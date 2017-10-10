#[macro_use]
extern crate log;
extern crate libc;
extern crate servo;

use servo::BrowserId;
use servo::Servo;
use servo::compositing::compositor_thread::EventLoopWaker;
use servo::compositing::windowing::{WindowEvent, WindowMethods};
use servo::euclid::{Point2D, ScaleFactor, Size2D, TypedPoint2D, TypedRect, TypedSize2D, TypedVector2D};
use servo::gl;
use servo::ipc_channel::ipc;
use servo::msg::constellation_msg::{Key, KeyModifiers};
use servo::net_traits::net_error_list::NetError;
use servo::script_traits::{LoadData, TouchEventType};
use servo::servo_config::opts;
use servo::servo_config::resource_files::set_resources_path;
use servo::servo_geometry::DeviceIndependentPixel;
use servo::servo_url::ServoUrl;
use servo::style_traits::DevicePixel;
use servo::style_traits::cursor::Cursor;
use servo::webrender_api;
use std::cell::RefCell;
use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::{c_char, c_void};
use std::rc::Rc;

#[allow(non_camel_case_types)]
mod egl {
    use super::libc;
    pub type khronos_utime_nanoseconds_t = libc::uint64_t;
    pub type khronos_uint64_t = libc::uint64_t;
    pub type khronos_ssize_t = libc::c_long;
    pub type EGLNativeDisplayType = *const libc::c_void;
    pub type EGLNativePixmapType = *const libc::c_void;
    pub type EGLint = libc::int32_t;
    pub type NativeDisplayType = *const libc::c_void;
    pub type NativePixmapType = *const libc::c_void;
    #[cfg(target_os = "android")]
    pub type NativeWindowType = *const libc::c_void;
    #[cfg(target_os = "android")]
    pub type EGLNativeWindowType = *const libc::c_void;
    include!(concat!(env!("OUT_DIR"), "/egl_bindings.rs"));
}

struct State {
    servo: Servo<Callbacks>,
    callbacks: Rc<Callbacks>,
    browser_id: BrowserId,
    events: Vec<WindowEvent>,
}

thread_local! {
    static SERVO: RefCell<Option<State>> = RefCell::new(None);
}

#[no_mangle]
pub extern "C" fn servo_version() -> *const c_char {
    let servo_version = servo::config::servo_version();
    let text = CString::new(servo_version).unwrap();
    let ptr = text.as_ptr();
    std::mem::forget(text);
    ptr
}

#[no_mangle]
pub extern "C" fn init_with_egl(
    wakeup: extern fn(),
    flush_cb: extern fn(),
    log_external: extern fn(*const c_char),
    width: u32,
    height: u32) {

    let _ = Logger::init(log_external);

    info!("init_with_egl");

    let gl = unsafe {
        gl::GlesFns::load_with(|addr| {
            let addr = CString::new(addr.as_bytes()).unwrap();
            let addr = addr.as_ptr();
            let egl = egl::Egl;
            egl.GetProcAddress(addr) as *const c_void
        })
    };

    init(gl, wakeup, flush_cb, width, height);
}

fn init(
    gl: Rc<gl::Gl>,
    wakeup: extern fn(),
    flush_cb: extern fn(),
    width: u32,
    height: u32) {

    set_resources_path(Some("/sdcard/servo/resources/".to_owned()));

    let opts = opts::default_opts();
    opts::set_defaults(opts);

    gl.clear_color(1.0, 0.0, 0.0, 1.0);
    gl.clear(gl::COLOR_BUFFER_BIT);
    gl.finish();

    let callbacks = Rc::new(Callbacks {
        waker: Box::new(SimpleEventLoopWaker(wakeup)),
        gl: gl.clone(),
        flush_cb: flush_cb,
        size: (width, height),
    });

    let mut servo = servo::Servo::new(callbacks.clone());

    let url = ServoUrl::parse("http://paulrouget.com").unwrap();
    let (sender, receiver) = ipc::channel().unwrap();
    servo.handle_events(vec![WindowEvent::NewBrowser(url, sender)]);
    let browser_id = receiver.recv().unwrap();
    servo.handle_events(vec![WindowEvent::SelectBrowser(browser_id)]);

    SERVO.with(|s| {
        *s.borrow_mut() = Some(State {
            servo,
            callbacks,
            browser_id,
            events: vec![],
        });
    });
}

#[no_mangle]
pub extern "C" fn on_event_loop_awaken_by_servo() {
    info!("on_event_loop_awaken_by_servo");
    SERVO.with(|s| {
        s.borrow_mut().as_mut().map(|ref mut s| {
            let events = mem::replace(&mut s.events, Vec::new());
            s.servo.handle_events(events);
        });
    });
}

#[no_mangle]
pub extern "C" fn load_url(url: *const c_char) {
    info!("load_url");
    SERVO.with(|s| {
        let url = unsafe { CStr::from_ptr(url) };
        if let Ok(url) = url.to_str() {
            if let Ok(url) = ServoUrl::parse(url) {
                s.borrow_mut().as_mut().map(|ref mut s| {
                    s.events.push(WindowEvent::LoadUrl(s.browser_id, url));
                });
            }
        }
    });
}

#[no_mangle]
pub extern "C" fn scroll(dx: i32, dy: i32, x: u32, y: u32, state: i32) {
    SERVO.with(|s| {
        s.borrow_mut().as_mut().map(|ref mut s| {
            let factor = s.callbacks.hidpi_factor().get();
            let dx = dx as f32 * factor;
            let dy = dy as f32 * factor;
            let x = x as f32 * factor;
            let y = y as f32 * factor;
            let delta = TypedVector2D::new(dx as f32, dy as f32);
            let scroll_location = webrender_api::ScrollLocation::Delta(delta);
            let phase = if state == 0 {
                TouchEventType::Down
            } else if state == 1 {
                TouchEventType::Move
            } else {
                TouchEventType::Up
            };
            let event = WindowEvent::Scroll(scroll_location, TypedPoint2D::new(x as i32, y as i32), phase);
            info!("SCROLL: {:?} {:?} {:?}", scroll_location, (x, y), phase);
            s.servo.handle_events(vec![event]);
        })
    });
}

pub struct SimpleEventLoopWaker(extern fn());

impl EventLoopWaker for SimpleEventLoopWaker {
    fn clone(&self) -> Box<EventLoopWaker + Send> {
        Box::new(SimpleEventLoopWaker(self.0))
    }
    fn wake(&self) {
        (self.0)();
    }
}

struct Callbacks {
    waker: Box<EventLoopWaker>,
    gl: Rc<gl::Gl>,
    flush_cb: extern fn(),
    size: (u32, u32),
}

impl WindowMethods for Callbacks {
    fn prepare_for_composite(&self, _width: usize, _height: usize) -> bool {
        true
    }

    fn present(&self) {
        (self.flush_cb)();
    }

    fn supports_clipboard(&self) -> bool {
        false
    }

    fn create_event_loop_waker(&self) -> Box<EventLoopWaker> {
        self.waker.clone()
    }

    fn gl(&self) -> Rc<gl::Gl> {
        self.gl.clone()
    }

    fn hidpi_factor(&self) -> ScaleFactor<f32, DeviceIndependentPixel, DevicePixel> {
        let factor = 2.0;
        ScaleFactor::new(factor)
    }

    fn framebuffer_size(&self) -> TypedSize2D<u32, DevicePixel> {
        let scale_factor = 2;
        TypedSize2D::new(scale_factor * self.size.0, scale_factor * self.size.1)
    }

    fn window_rect(&self) -> TypedRect<u32, DevicePixel> {
        TypedRect::new(TypedPoint2D::new(0, 0), self.framebuffer_size())
    }

    fn size(&self) -> TypedSize2D<f32, DeviceIndependentPixel> {
        TypedSize2D::new(self.size.0 as f32, self.size.1 as f32)
    }

    fn client_window(&self, _id: BrowserId) -> (Size2D<u32>, Point2D<i32>) {
        (Size2D::new(self.size.0, self.size.1), Point2D::new(0, 0))
    }

    fn allow_navigation(&self, _id: BrowserId, _url: ServoUrl, chan: ipc::IpcSender<bool>) { chan.send(true).ok(); }
    fn set_inner_size(&self, _id: BrowserId, _size: Size2D<u32>) {}
    fn set_position(&self, _id: BrowserId, _point: Point2D<i32>) {}
    fn set_fullscreen_state(&self, _id: BrowserId, _state: bool) {}
    fn set_page_title(&self, _id: BrowserId, _title: Option<String>) {}
    fn status(&self, _id: BrowserId, _status: Option<String>) {}
    fn load_start(&self, _id: BrowserId) {}
    fn load_end(&self, _id: BrowserId) {}
    fn load_error(&self, _id: BrowserId, _: NetError, _url: String) {}
    fn head_parsed(&self, _id: BrowserId) {}
    fn history_changed(&self, _id: BrowserId, _entries: Vec<LoadData>, _current: usize) {}
    fn set_cursor(&self, _cursor: Cursor) { }
    fn set_favicon(&self, _id: BrowserId, _url: ServoUrl) {}
    fn handle_key(&self, _id: Option<BrowserId>, _ch: Option<char>, _key: Key, _mods: KeyModifiers) { }
}



use log::{set_logger, Log, LogRecord, LogMetadata, LogLevelFilter, LogLevel};

struct Logger(extern fn(*const c_char));

impl Logger {
    pub fn init(callback: extern fn(*const c_char)) {
        set_logger(|max_log_level| {
            max_log_level.set(LogLevelFilter::Info);
            Box::new(Logger(callback))
        }).expect("set_logger failed");
    }
}

impl Log for Logger {
    fn enabled(&self, metadata: &LogMetadata) -> bool {
        metadata.level() <= LogLevel::Debug
    }

    fn log(&self, record: &LogRecord) {
        if self.enabled(record.metadata()) {
            let msg = format!("LOG: [{}] [{}] [{}]", record.level(), record.args(), record.target());
            let text = CString::new(msg.to_owned()).unwrap();
            let ptr = text.as_ptr();
            self.0(ptr);
        }
    }
}

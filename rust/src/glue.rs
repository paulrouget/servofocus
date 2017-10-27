/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

use api::*;
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
use servo;
use std::cell::RefCell;
use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::c_char;
use std::rc::Rc;

thread_local! {
    pub static SERVO: RefCell<Option<State>> = RefCell::new(None);
}

pub struct State {
    servo: Servo<ServoCallbacks>,
    callbacks: Rc<ServoCallbacks>,
    browser_id: BrowserId,
    events: Vec<WindowEvent>,
}

pub fn servo_version() -> *const c_char {
    let servo_version = servo::config::servo_version();
    let text = CString::new(servo_version).unwrap();
    let ptr = text.as_ptr();
    mem::forget(text);
    ptr
}

pub fn init(
    gl: Rc<gl::Gl>,
    callbacks: HostCallbacks,
    layout: ViewLayout) -> ServoResult {

    // FIXME: for now, don't rely on layout
    let layout = ViewLayout {
        view_size: Size { width: 50, height: 50 },
        margins: Margins { top: 0, right: 0, bottom: 0, left: 0},
        position: Position { x: 0, y: 0 },
        hidpi_factor: 2.0,
    };

    info!("glue::init 1");

    set_resources_path(Some("/sdcard/servo/resources/".to_owned()));

    let opts = opts::default_opts();
    opts::set_defaults(opts);

    info!("glue::init 2");

    gl.clear_color(1.0, 1.0, 1.0, 1.0);
    gl.clear(gl::COLOR_BUFFER_BIT);
    gl.finish();

    info!("glue::init 3");

    let callbacks = Rc::new(ServoCallbacks {
        waker: Box::new(RemoteEventLoopWaker(callbacks.wakeup)),
        gl: gl.clone(),
        flush: callbacks.flush,
        layout,
    });

    info!("glue::init 4");

    let mut servo = servo::Servo::new(callbacks.clone());

    let url = ServoUrl::parse("about:not-found").unwrap();
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

    info!("glue::init::finished");

    ServoResult::Ok
}

impl State {
    pub fn perform_updates(&mut self) -> ServoResult {
        info!("perform_updates");
        let events = mem::replace(&mut self.events, Vec::new());
        self.servo.handle_events(events);
        ServoResult::Ok
    }

    pub fn load_url(&mut self, url: *const c_char) -> ServoResult {
        info!("load_url");
        let url = unsafe { CStr::from_ptr(url) };
        url.to_str()
           .map_err(|_| ServoResult::CantReadStr)
           .and_then(|txt| ServoUrl::parse(txt).map_err(|_| ServoResult::CantParseUrl))
           .map(|url| self.events.push(WindowEvent::LoadUrl(self.browser_id, url)))
           .map(|_| ServoResult::Ok)
           .unwrap_or_else(|err| err)
    }

    pub fn scroll(&mut self, dx: i32, dy: i32, x: u32, y: u32, state: ScrollState) -> ServoResult {
        let factor = self.callbacks.hidpi_factor().get();
        let dx = dx as f32 * factor;
        let dy = dy as f32 * factor;
        let x = x as f32 * factor;
        let y = y as f32 * factor;
        let delta = TypedVector2D::new(dx as f32, dy as f32);
        let scroll_location = webrender_api::ScrollLocation::Delta(delta);
        let phase = match state {
            ScrollState::Start => TouchEventType::Down,
            ScrollState::Move => TouchEventType::Move,
            ScrollState::End => TouchEventType::Up,
            ScrollState::Canceled => TouchEventType::Cancel,
        };
        let event = WindowEvent::Scroll(scroll_location, TypedPoint2D::new(x as i32, y as i32), phase);
        self.servo.handle_events(vec![event]);
        ServoResult::Ok
    }
}


pub struct RemoteEventLoopWaker(extern fn());

impl EventLoopWaker for RemoteEventLoopWaker {
    fn clone(&self) -> Box<EventLoopWaker + Send> {
        Box::new(RemoteEventLoopWaker(self.0))
    }
    fn wake(&self) {
        (self.0)();
    }
}

struct ServoCallbacks {
    waker: Box<EventLoopWaker>,
    gl: Rc<gl::Gl>,
    flush: extern fn(),
    layout: ViewLayout,
}

impl WindowMethods for ServoCallbacks {
    fn prepare_for_composite(&self, _width: usize, _height: usize) -> bool {
        true
    }

    fn present(&self) {
        (self.flush)();
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
        ScaleFactor::new(self.layout.hidpi_factor)
    }

    fn framebuffer_size(&self) -> TypedSize2D<u32, DevicePixel> {
        TypedSize2D::new(self.layout.view_size.width, self.layout.view_size.height)
    }

    fn window_rect(&self) -> TypedRect<u32, DevicePixel> {
        TypedRect::new(TypedPoint2D::new(0, 0), self.framebuffer_size())
    }

    fn size(&self) -> TypedSize2D<f32, DeviceIndependentPixel> {
        let width = self.layout.view_size.width as f32;
        let height = self.layout.view_size.height as f32;
        let factor = self.layout.hidpi_factor;
        TypedSize2D::new(width / factor, height / factor)
    }

    fn client_window(&self, _id: BrowserId) -> (Size2D<u32>, Point2D<i32>) {
        let factor = self.layout.hidpi_factor;
        let width: u32 = (self.layout.view_size.width as f32 / factor) as u32;
        let height: u32 = (self.layout.view_size.height as f32 / factor) as u32;
        (Size2D::new(width, height), Point2D::new(0, 0))
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

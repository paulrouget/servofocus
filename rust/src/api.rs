/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//! Generic result codes

use logs::Logger;
use gl_glue;
use glue::{self, SERVO};

/// Generic result errors
#[repr(C)]
pub enum ServoResult {
    Ok,
    UnexpectedError,
    WrongThread,
    CantReadStr,
    CantParseUrl,    
}

/// Scroll state
#[repr(C)]
pub enum ScrollState {
    Start,
    Move,
    End,
    Canceled,
}

/// Callback used by Servo internals
#[repr(C)]
pub struct HostCallbacks {

    /// Will be called from any thread.
    /// Will be called to notify embedder that some events
    /// are available, and that perform_updates need to be called
    pub wakeup: extern fn(),

    /// Will be called from the thread used for the init call
    /// Will be called when the GL buffer has been updated.
    pub flush: extern fn(),

    /// Will be call from any thread.
    /// Used to report logging.
    /// Warning: this might be called a lot.
    pub log: extern fn(*const u8),

}

#[repr(C)]
pub struct Size {
    pub width: u32,
    pub height: u32,
}

#[repr(C)]
pub struct Margins {
    pub top: u32,
    pub right: u32,
    pub bottom: u32,
    pub left: u32,
}

#[repr(C)]
pub struct Position {
    pub x: i32,
    pub y: i32,
}

#[repr(C)]
pub struct ViewLayout {
    /// Size of the view. Hardware pixels.
    pub view_size: Size,
    /// Margins of the view. Hardware pixels.
    /// Pages are painted all over the surface,
    /// but if margins are not zero, the layout
    /// coordinates are bounds byt these margins.
    pub margins: Margins,
    /// Position of the window.
    pub position: Position,
    /// Pixel density.
    pub hidpi_factor: f32,
}

#[no_mangle]
pub extern "C" fn servo_version() -> *const u8 {
    glue::servo_version()
}

/// Needs to be called from the EGL thread
#[no_mangle]
pub extern "C" fn init_with_egl(callbacks: HostCallbacks, layout: ViewLayout) -> ServoResult {
    let _ = Logger::init(callbacks.log);
    gl_glue::init_with_egl(callbacks, layout)
}

/// This is the Servo heartbeat. This needs to be called
/// everytime wakeup is called.
#[no_mangle]
pub extern "C" fn perform_updates() -> ServoResult {
    let mut res = ServoResult::UnexpectedError;
    SERVO.with(|s| {
        res = s.borrow_mut().as_mut().map(|ref mut s| {
            s.perform_updates()
        }).unwrap_or(ServoResult::WrongThread)
    });
    res
}

/// Load an URL. This needs to be a valid url.
#[no_mangle]
pub extern "C" fn load_url(url: *const u8) -> ServoResult {
    let mut res = ServoResult::UnexpectedError;
    SERVO.with(|s| {
        res = s.borrow_mut().as_mut().map(|ref mut s| {
            s.load_url(url)
        }).unwrap_or(ServoResult::WrongThread)
    });
    res
}

#[no_mangle]
pub extern "C" fn scroll(dx: i32, dy: i32, x: u32, y: u32, state: ScrollState) -> ServoResult {
    let mut res = ServoResult::UnexpectedError;
    SERVO.with(|s| {
        res = s.borrow_mut().as_mut().map(|ref mut s| {
            s.scroll(dx, dy, x, y, state)
        }).unwrap_or(ServoResult::WrongThread)
    });
    res
}


/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

use servo::gl;
use std::ffi::CString;
use std::os::raw::c_void;
use std::rc::Rc;

#[allow(non_camel_case_types)]
mod egl {
    use libc;
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

pub fn init_egl() -> Rc<gl::Gl> {
    info!("init_egl");
    unsafe {
        gl::GlesFns::load_with(|addr| {
            let addr = CString::new(addr.as_bytes()).unwrap();
            let addr = addr.as_ptr();
            let egl = egl::Egl;
            egl.GetProcAddress(addr) as *const c_void
        })
    }
}




#ifndef cheddar_generated_libservobridge_h
#define cheddar_generated_libservobridge_h


#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>
#include <stdbool.h>



/// Generic result errors
typedef enum ServoResult {
	ServoResult_Ok,
	ServoResult_UnexpectedError,
	ServoResult_WrongThread,
	ServoResult_CantReadStr,
	ServoResult_CantParseUrl,
	ServoResult_NotImplemented,
} ServoResult;

/// Scroll state
typedef enum ScrollState {
	ScrollState_Start,
	ScrollState_Move,
	ScrollState_End,
	ScrollState_Canceled,
} ScrollState;

/// Touch state
typedef enum TouchState {
	TouchState_Down,
	TouchState_Up,
} TouchState;

/// Callback used by Servo internals
typedef struct HostCallbacks {
	/// Will be called from any thread.
	/// Will be called to notify embedder that some events
	/// are available, and that perform_updates need to be called
	void (*wakeup)(void);
	/// Will be called from the thread used for the init call
	/// Will be called when the GL buffer has been updated.
	void (*flush)(void);
	/// Will be call from any thread.
	/// Used to report logging.
	/// Warning: this might be called a lot.
	void (*log)(c_char const* log);
	/// Page starts loading.
	/// "Reload button" becomes "Stop button".
	/// Throbber starts spinning.
	void (*on_load_started)(void);
	/// Page has loaded.
	/// "Stop button" becomes "Reload button".
	/// Throbber stops spinning.
	void (*on_load_ended)(void);
	/// Title changed.
	void (*on_title_changed)(c_char const* title);
	/// URL changed.
	void (*on_url_changed)(c_char const* url);
	/// Back/forward state changed.
	/// Back/forward buttons need to be disabled/enabled.
	void (*on_history_changed)(bool can_go_back, bool can_go_forward);
} HostCallbacks;

typedef struct Size {
	uint32_t width;
	uint32_t height;
} Size;

typedef struct Margins {
	uint32_t top;
	uint32_t right;
	uint32_t bottom;
	uint32_t left;
} Margins;

typedef struct Position {
	int32_t x;
	int32_t y;
} Position;

typedef struct ViewLayout {
	/// Size of the view. Hardware pixels.
	Size view_size;
	/// Margins of the view. Hardware pixels.
	/// Pages are painted all over the surface,
	/// but if margins are not zero, the layout
	/// coordinates are bounds by these margins.
	Margins margins;
	/// Position of the window.
	Position position;
	/// Pixel density.
	float hidpi_factor;
} ViewLayout;

c_char const* servo_version(void);

/// Needs to be called from the EGL thread
ServoResult init_with_egl(c_char const* url, c_char const* resources_path, HostCallbacks callbacks, ViewLayout layout);

/// Needs to be called from the main thread
ServoResult init_with_gl(c_char const* url, c_char const* resources_path, HostCallbacks callbacks, ViewLayout layout);

/// This is the Servo heartbeat. This needs to be called
/// everytime wakeup is called.
ServoResult perform_updates(void);

ServoResult scroll(int32_t dx, int32_t dy, uint32_t x, uint32_t y, ScrollState state);

ServoResult click(uint32_t x, uint32_t y);

/// Load an URL. This needs to be a valid url.
ServoResult load_url(c_char const* url);

/// Reload page.
ServoResult reload(void);

/// Reload page.
ServoResult resize(ViewLayout layout);

/// Stop page loading.
ServoResult stop(void);

ServoResult go_back(void);

ServoResult go_forward(void);



#ifdef __cplusplus
}
#endif


#endif

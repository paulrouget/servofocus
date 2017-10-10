
#ifndef cheddar_generated_libsimpleservo_h
#define cheddar_generated_libsimpleservo_h


#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>
#include <stdbool.h>



c_char const* servo_version(void);

void init_with_egl(void (*wakeup)(void), void (*flush_cb)(void), void (*log_external)(c_char const* ), uint32_t width, uint32_t height);

void on_event_loop_awaken_by_servo(void);

void load_url(c_char const* url);

void scroll(int32_t dx, int32_t dy, uint32_t x, uint32_t y, int32_t state);



#ifdef __cplusplus
}
#endif


#endif

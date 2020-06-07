namespace AuraSharp
{
    public enum AuraEffect: byte
    {
        OFF = 0x00,                         /* OFF mode                             */
        STATIC = 0x01,                      /* Static color mode                    */
        BREATHING = 0x02,                   /* Breathing effect mode                */
        FLASHING = 0x03,                    /* Flashing effect mode                 */
        SPECTRUM_CYCLE = 0x04,              /* Spectrum Cycle mode                  */
        RAINBOW = 0x05,                     /* Rainbow effect mode                  */
        SPECTRUM_CYCLE_BREATHING = 0x06,    /* Rainbow Breathing effect mode        */
        CHASE_FADE = 0x07,                  /* Chase with Fade effect mode          */
        SPECTRUM_CYCLE_CHASE_FADE = 0x08,   /* Chase with Fade, Rainbow effect mode */
        CHASE = 0x09,                       /* Chase effect mode                    */
        SPECTRUM_CYCLE_CHASE = 0x0A,        /* Chase with Rainbow effect mode       */
        SPECTRUM_CYCLE_WAVE = 0x0B,         /* Wave effect mode                     */
        CHASE_RAINBOW_PULSE = 0x0C,         /* Chase with Rainbow Pulse effect mode */
        RANDOM_FLICKER = 0x0D,              /* Random flicker effect mode           */
        MUSIC = 0x0E,                       /* Music Mode                           */
        DIRECT = 0xFF                       /* Direct Mode                          */
    }   
}
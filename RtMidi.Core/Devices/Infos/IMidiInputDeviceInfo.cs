namespace RtMidi.Core.Devices.Infos
{
    /// <summary>
    /// Provides information about an available MIDI Input device
    /// </summary>
    public interface IMidiInputDeviceInfo : IMidiDeviceInfo
    {
        /// <summary>
        /// Create MIDI Input device used to receive midi messages for this device
        /// </summary>
        /// <param name="ignoreMidiTime">Indicates if MIDI time events should be ignored</param>
        /// <returns>The device.</returns>
        IMidiInputDevice CreateDevice(bool ignoreMidiTime = true, bool onlyRaw = false);
    }
}

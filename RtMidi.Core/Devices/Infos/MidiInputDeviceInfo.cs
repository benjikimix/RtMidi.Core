using RtMidi.Core.Unmanaged.Devices.Infos;
namespace RtMidi.Core.Devices.Infos
{
    internal class MidiInputDeviceInfo : MidiDeviceInfo<RtMidiInputDeviceInfo>, IMidiInputDeviceInfo
    {
        public MidiInputDeviceInfo(RtMidiInputDeviceInfo rtMidiDeviceInfo) : base(rtMidiDeviceInfo)
        {
        }

        public IMidiInputDevice CreateDevice(bool ignoreMidiTime = true)
        {
            return new MidiInputDevice(RtMidiDeviceInfo.CreateDevice(ignoreMidiTime), Name);
        }
    }
}

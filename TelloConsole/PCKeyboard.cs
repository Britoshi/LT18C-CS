using SharpDX.DirectInput;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelloConsole
{
    class PCKeyboard
    {
        public static KeyboardState joyState = new KeyboardState();
        public delegate void updateDeligate(KeyboardState state);
        public static event updateDeligate onUpdate;

        public static void init()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var keyboardGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Keyboard,
                        DeviceEnumerationFlags.AllDevices))
                keyboardGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (keyboardGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Keyboard,
                        DeviceEnumerationFlags.AllDevices))
                    keyboardGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, return.
            if (keyboardGuid == Guid.Empty)
            {
                Console.WriteLine("No Keyboard found.");
                return;
            }

            // Instantiate the joystick
            var keyboard = new Keyboard(directInput);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", keyboardGuid);

            // Query all suported ForceFeedback effects
            var allEffects = keyboard.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            keyboard.Properties.BufferSize = 128;

            // Acquire the joystick
            keyboard.Acquire();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {

                    keyboard.Poll();
                    keyboard.GetCurrentState(ref joyState);
                    onUpdate(joyState);
                    Thread.Sleep(10); 
                }
            });
        }
    }
}

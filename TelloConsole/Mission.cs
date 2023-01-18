using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelloConsole;
using TelloLib;

namespace Brito
{
    public class Mission
    {

        //<18c or lt18c or less than 18 credits
        private static Mission _instance;
        public static Mission Instance
        {
            get
            {
                if (_instance == null) _instance = new Mission();
                return _instance;
            }
        }

        bool lockHeight = false;
        float targetHeight;

        public Single[] AxisModification;

        public Mission()
        {
            lockHeight = false;
            targetHeight = 100f;
            AxisModification = new Single[5] { 0f, 0f, 0f, 0f, 0f };
        }

        public static void ModLX(Single x) => Instance.AxisModification[0] = x;
        public static void ModLY(Single y) => Instance.AxisModification[1] = y;
        public static void ModRX(Single x) => Instance.AxisModification[2] = x;
        public static void ModRY(Single y) => Instance.AxisModification[3] = y;

        public static void ModAxis(Single lx, Single ly, Single rx, Single ry)
        {
            Mission.ModLX(lx);
            Mission.ModLY(ly);
            Mission.ModRX(rx);
            Mission.ModRY(ry);
        }

        public static void Update() => Instance._update();
        private void _update()
        {

            if (lockHeight)
                LockHeightHandler();


        }
        static void printAt(int x, int y, string str)
        {
            var saveLeft = Console.CursorLeft;
            var saveTop = Console.CursorTop;
            Console.SetCursorPosition(x, y);
            Console.WriteLine(str + "     ");//Hack. extra space is to clear any previous chars.
            Console.SetCursorPosition(saveLeft, saveTop);

        }

        private void UpdatePosition()
        {

            if (PCKeyboard.joyState.IsPressed(Key.A) ||
                PCKeyboard.joyState.IsPressed(Key.D) ||
                PCKeyboard.joyState.IsPressed(Key.W) ||
                PCKeyboard.joyState.IsPressed(Key.S) ||
                PCKeyboard.joyState.IsPressed(Key.Q) ||
                PCKeyboard.joyState.IsPressed(Key.E) || 
                PCKeyboard.joyState.IsPressed(Key.Space) || 
                PCKeyboard.joyState.IsPressed(Key.LeftControl))
                return;

            //Temp
            Tello.controllerState.setAxis(
                AxisModification[0],
                AxisModification[1],
                AxisModification[2],
                AxisModification[3]);
            Tello.sendControllerUpdate();
        }
        private void LockHeightHandler()
        {
            var height = Tello.state.height;
            var difference = targetHeight - (height * 10);

            printAt(0, 3, "Height: " + (height * 10));
            printAt(0,0,"Diff: " + difference);

            var threshHold = 1f;

            if (difference > threshHold)
            {

                Single amount = difference;
                if (amount > 1) amount = 1;
                else if (amount < -1) amount = -1;

                ModLY(amount);

                printAt(0, 6, "Modding: " + amount);
            }
            else
                ModLY(0);

            //UpdatePosition();

        }

        public static void SetLockHeight(float height)
        {
            Instance.lockHeight = true;
            Instance.targetHeight = height;
        }
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using System;
using System.Threading.Tasks;
using CitizenFX.Core.UI;

namespace seatbelt
{
    public class Seatbelt : BaseScript
    {
        // Some config settings
        private bool showNotification = true;       // Whether to use Screen.ShowNotification on seatbelt status change
        // End configuration

        private bool isUIOpen = false;
        private float[] speedBuffer = new float[2];
        private Vector3[] velBuffer = new Vector3[2];
        private bool beltOn = false;
        private bool wasInCar = false;

        public Seatbelt()
        {
            Tick += OnTick;
            Tick += DeathTick;
        }

        private bool IsCar(int vehicle)
        {
            var vc = GetVehicleClass(vehicle);

            return (vc >= 0 && vc <= 7) || (vc >= 9 && vc <= 12) || (vc >= 17 && vc <= 20);
        }

        private async Task OnTick()
        {
            int _ped = GetPlayerPed(-1);
            int _vehicle = GetVehiclePedIsIn(_ped, false);
            if (_vehicle > 0 && (wasInCar || IsCar(_vehicle)))
            {
                wasInCar = true;
                if (!isUIOpen && !IsPlayerDead(_ped))
                {
                    isUIOpen = true;
                    NUIBuckled(beltOn);
                }

                // If seat belt is on, keep them from exiting vehicle
                if (beltOn)
                {
                    // Disables INPUT_VEH_EXIT // default: "F"
                    DisableControlAction(0, 75, true);
                }

                speedBuffer[1] = speedBuffer[0];
                speedBuffer[0] = GetEntitySpeed(_vehicle);

                if (speedBuffer[1] > 0.0f &&
                    !beltOn &&
                    GetEntitySpeedVector(_vehicle, true).Y > 1.0f &&
                    speedBuffer[0] > 15.0f &&               // m/s which comes out to approx 34mph
                    (speedBuffer[1] - speedBuffer[0]) > (speedBuffer[0] * 0.255f))
                {
                    var coords = GetEntityCoords(_ped, true);
                    var fw = ForwardVelocity(_ped);
                    SetEntityCoords(_ped, coords.X + fw[0], coords.Y + fw[1], coords.Z - 0.47f, true, true, true, true);
                    SetEntityVelocity(_ped, velBuffer[1].X, velBuffer[1].Y, velBuffer[1].Z);
                    await Delay(1);
                    SetPedToRagdoll(GetPlayerPed(-1), 3000, 3000, 0, false, false, false);
                }

                velBuffer[1] = velBuffer[0];
                velBuffer[0] = GetEntityVelocity(_vehicle);

                // Key is "Ctrl+Z"
                if (IsControlJustPressed(36, 20))
                {
                    beltOn = !beltOn;
                    if (beltOn && showNotification) Screen.ShowNotification("Seatbelt On");
                    else if (!beltOn && showNotification) Screen.ShowNotification("Seatbelt Off");
                    NUIBuckled(beltOn);
                }
            }
            else if (wasInCar)
            {
                wasInCar = false;
                beltOn = false;
                speedBuffer[0] = speedBuffer[1] = 0.0f;
                if (isUIOpen && !IsPlayerDead(_ped))
                {
                    isUIOpen = false;
                }

                // Make sure chime is off.
                NUIBuckled(beltOn);
            }

            await Delay(0);
        }

        private float[] ForwardVelocity(int ent)
        {
            float hdg = GetEntityHeading(ent);
            if (hdg < 0.0) hdg += 360.0f;

            hdg = hdg * 0.0174533f;

            float[] ret = new float[2];
            ret[0] = (float)Math.Cos(hdg) * 2.0f;
            ret[1] = (float) Math.Sin(hdg) * 2.0f;

            return ret;
        }

        private async Task DeathTick()
        {
            if (API.IsPlayerDead(API.PlayerId()) && isUIOpen)
            {
                isUIOpen = false;
            }
            await Delay(100);
        }

        private void NUIBuckled(bool value)
        {
            SendNUIMessage("{\"transactionType\":\"isBuckled\",\"transactionValue\":" + value.ToString().ToLower() +
                           ", \"inCar\":" + wasInCar.ToString().ToLower() + "}");
        }

        private void SendNUIMessage(string message)
        {
            SendNuiMessage(message);
        }
    }
}

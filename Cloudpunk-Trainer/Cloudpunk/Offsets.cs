using Cloudpunk_Trainer.Models;
using System.ComponentModel;

namespace Cloudpunk_Trainer.Cloudpunk
{
    public static class Offsets
    {
        public static BindingList<Offset> global = new BindingList<Offset>()
        {
            new Offset("idleMusicTimer", typeof(float), 0xC0),
            new Offset("advertisementTimer", typeof(float), 0xC4),
            new Offset("makeCallTimer", typeof(float), 0x4D4),
            new Offset("money", typeof(int), 0x508),
            new Offset("numLocationsUnlocked", typeof(int), 0x5A0),
            new Offset("numRepairs", typeof(int), 0x5A4),
            new Offset("joosTimePassed", typeof(float), 0x610),
            new Offset("joosTimeLeft", typeof(float), 0x614),
            new Offset("alcoholTimePassed", typeof(float), 0x640),
            new Offset("alcoholTimeLeft", typeof(float), 0x644),
            new Offset("speedGainTimeLeft", typeof(float), 0x648),
            new Offset("stimsTimePassed", typeof(float), 0x64C),
            new Offset("stimsTimeLeft", typeof(float), 0x650),
            new Offset("pheromonesTimePassed", typeof(float), 0x654),
            new Offset("pheromonesTimeLeft", typeof(float), 0x658),
            new Offset("foodCooldown", typeof(float), 0x65C),
            new Offset("drinkCooldown", typeof(float), 0x660),
            new Offset("drugCooldown", typeof(float), 0x664),
            new Offset("timeSinceStart", typeof(float), 0x718)
        };

        public static BindingList<Offset> player = new BindingList<Offset>()
        {
            new Offset("health", typeof(int), 0x40),
            new Offset("intendedZoomFactor", typeof(float), 0x128),
            new Offset("runSpeed", typeof(float), 0x190),
            new Offset("sneakSpeed", typeof(float), 0x194),
            new Offset("gravity", typeof(float), 0x198),
        };

        public static BindingList<Offset> playerCar { get; } = new BindingList<Offset>()
        {
            new Offset("id", typeof(int), 0x18),
            new Offset("maxStatus", typeof(int), 0x48),
            new Offset("maxFuel", typeof(int), 0x4C),
            new Offset("stabilizerConstant", typeof(float), 0x80),
            new Offset("stabilizerDamperConstant", typeof(float), 0x84),
            new Offset("acceleration", typeof(float), 0x90),
            new Offset("minMotorTorque", typeof(float), 0x94),
            new Offset("motorTorque", typeof(float), 0x98),
            new Offset("midMotorTorque", typeof(float), 0x9C),
            new Offset("speedwayMotorTorque", typeof(float), 0xA0),
            new Offset("minSteerTorque", typeof(float), 0xA4),
            new Offset("steerTorque", typeof(float), 0xA8),
            new Offset("midSteerTorque", typeof(float), 0xAC),
            new Offset("speedwaySteerTorque", typeof(float), 0xB0),
            new Offset("minMaximumSpeed", typeof(float), 0xB4),
            new Offset("maximumSpeed", typeof(float), 0xB8),
            new Offset("midMaximumSpeed", typeof(float), 0xBC),
            new Offset("maximumSpeedHighway", typeof(float), 0xC0),
            new Offset("currentMaxSpeed", typeof(float), 0xC4),
            new Offset("currentMotorTorque", typeof(float), 0xC8),
            new Offset("currentSteerTorque", typeof(float), 0xCC),
            new Offset("currentSpeed", typeof(float), 0xD0),
            new Offset("stableHeight", typeof(float), 0xD4),
            new Offset("maximumAngularVelocity", typeof(float), 0xD8),
            new Offset("stability", typeof(float), 0xDC),
            new Offset("reflection", typeof(float), 0xE0),
            new Offset("speedBoostTimer", typeof(float), 0x198),
            new Offset("currentFloorLevel", typeof(float), 0x200),
            new Offset("currentStatus", typeof(float), 0x2B8),
            new Offset("currentFuel", typeof(float), 0x2C0),
            new Offset("layerMinY", typeof(float), 0x2EC),
            new Offset("layerMaxY", typeof(float), 0x2F0),
            new Offset("verticalSpeed", typeof(float), 0x2F4),
            new Offset("nextFuelTaskCheck", typeof(float), 0x3EC),
        };
    }
}

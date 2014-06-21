#region Using

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;

#endregion

namespace BusRoute
{
    public class BusRoute : Script
    {
        public class Bus
        {
            private readonly Vehicle _bus;
            private Ped _driver;
            private readonly List<Ped> _passagers;
            private Ped _passagerDest;
            private Blip _blip;

            public Vehicle BusVehicle
            {
                get { return this._bus; }
            }

            public Ped Driver
            {
                get { return this._driver; }
            }

            public List<Ped> Passagers
            {
                get { return this._passagers; }
            }

            public Ped Destination
            {
                get { return this._passagerDest; }
            }

            public bool Driving { get; set; }

            public Bus( Vector3 spawnPos, Vector3 _dest )
                : this()
            {
                this._bus = World.CreateVehicle( "BANSHEE", spawnPos );
                this._bus.Rotation = new Vector3( 0, 0, 180 );
                this._driver = this._bus.CreatePedOnSeat( VehicleSeat.Driver );
                this._passagers = new List<Ped>();
                this._passagerDest = World.CreatePed( _dest );
            }

            public Bus()
            {
            }

            private static Gender GetRandomGender()
            {
                Random random = new Random();
                return random.Next( 1, 100 ) >= 50 ? Gender.Male : Gender.Female;
            }

            /// <summary>
            /// Создает пассажира для автобуса
            /// </summary>
            public Ped CreatePassager()
            {
                Ped passager = World.CreatePed( this._bus.Position.Around( 20F ), GetRandomGender() );
                var tasks = new TaskSequence();
                tasks.AddTask.EnterVehicle( this._bus, VehicleSeat.AnyPassengerSeat );
                passager.Task.PerformSequence( tasks );
                this._passagers.Add( passager );
                return passager;
            }
        }

        //private readonly Vector3 _busSpawnVector = new Vector3( 1039.08F, 280.66F, 30.45F );
        private readonly Vector3 _busSpawnVector = new Vector3( 1087.16F, 197.99F, 30.72F );
        private readonly List<Bus> _busses;
        private readonly Dictionary<Bus, Blip> _blips;

        public BusRoute()
        {
            this.Interval = 50;
            this.Tick += this.BusRoute_Tick;
            this.KeyDown += new GTA.KeyEventHandler( this.BasicKeyExample_KeyDown );
            this._busses = new List<Bus>();
            this._blips = new Dictionary<Bus, Blip>();
        }

        private void BusRoute_Tick( object sender, EventArgs e )
        {
            foreach ( Bus bus in this._busses )
            {
                if ( !bus.Driver.isSittingInVehicle( bus.BusVehicle ) )
                {
                    continue;
                }
                bus.Driver.Task.ClearAll();
                bus.Driver.Task.AlwaysKeepTask = true;
                //bus.Driver.Task.DriveTo( bus.BusVehicle, World.GetNextPositionOnStreet( bus.Destination ), 35, false, true );
                bus.Driver.Task.DriveTo( bus.BusVehicle, bus.Destination, 35, true, true );
            }
        }

        private void BasicKeyExample_KeyDown( object sender, GTA.KeyEventArgs e )
        {
            if ( e.Key == Keys.F6 )
            {
                this.SpawnBusWithDriver();
            }
        }

        private void WaitWhilePedNotInCar( Ped ped, Vehicle vehicle )
        {
            if ( !ped.isInVehicle( vehicle ) )
            {
                this.Wait( 1 );
            }
        }

        private void SpawnBusWithDriver()
        {
            Bus bus = new Bus( this._busSpawnVector,
                new Vector3( this.Player.Character.Position.X, this.Player.Character.Position.Y,
                    this.Player.Character.Position.Z ) );
            this._busses.Add( bus );
            Blip blip = Blip.AddBlip( bus.BusVehicle );
            blip.Color = BlipColor.DarkTurquoise;
            blip.Name = "Bus";
            this._blips.Add( bus, blip );
        }
    }
}
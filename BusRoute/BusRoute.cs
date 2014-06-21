#region Using

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using Timer = GTA.Timer;

#endregion

namespace BusRoute
{
    public class BusRoute : Script
    {
        public class Bus
        {
            private readonly Vehicle _bus;
            private Ped _driver;
            private readonly List<Ped> _passengers;
            private Ped _passengerDest;
            private Blip _blip;
            private Timer _timer;
            private bool _waitForPassenger = false;

            #region Поля

            public Vehicle BusVehicle
            {
                get { return this._bus; }
            }

            public Ped Driver
            {
                get { return this._driver; }
            }

            public List<Ped> Passengers
            {
                get { return this._passengers; }
            }

            public Ped Destination
            {
                get { return this._passengerDest; }
            }

            public bool Driving { get; set; }

            public bool WaitForPassenger
            {
                get { return this._waitForPassenger; }
            }

            #endregion

            public Bus( Vector3 spawnPos, Vector3 dest )
            {
                this._timer = new Timer { Interval = 300 };
                this._timer.Tick += this._timer_Tick;
                this._timer.Start();

                this._bus = World.CreateVehicle( "BUS", spawnPos );
                this._bus.Rotation = new Vector3( 0, 0, 180 );
                this._driver = this._bus.CreatePedOnSeat( VehicleSeat.Driver );
                this._passengers = new List<Ped>();
                this._passengerDest = World.CreatePed( dest, GetRandomGender() );
                this._passengerDest.Invincible = true;
            }

            private void _timer_Tick( object sender, EventArgs e )
            {
                if ( this._bus.Position.DistanceTo( this._passengerDest.Position ) <= 6F && !this._waitForPassenger )
                {
                    Game.DisplayText( "reached" );
                    this._waitForPassenger = true;
                    this._driver.Task.ClearAll();
                    this._bus.Speed = 0;
                    this._passengerDest.Task.EnterVehicle( this._bus, this._bus.GetFreePassengerSeat() );
                }
                if ( this._waitForPassenger )
                {
                    if ( this._passengerDest.isInVehicle( this._bus ) )
                    {
                        this._passengers.Add( this._passengerDest );
                        this._passengerDest.Invincible = false;
                        this._passengerDest = World.CreatePed( new Vector3( 1087.16F, 197.99F, 30.72F ),
                            GetRandomGender() );
                        this._passengerDest.Invincible = true;
                        this._waitForPassenger = false;
                    }
                }
            }

            private static Gender GetRandomGender()
            {
                Random random = new Random();
                return random.Next( 1, 100 ) >= 50 ? Gender.Male : Gender.Female;
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
                if ( !bus.WaitForPassenger )
                {
                    bus.Driver.Task.ClearAll();
                    bus.Driver.Task.AlwaysKeepTask = true;
                    //bus.Driver.Task.DriveTo( bus.BusVehicle, World.GetNextPositionOnStreet( bus.Destination ), 35, false, true );
                    bus.Driver.Task.DriveTo( bus.BusVehicle, bus.Destination, 15, true, false );
                }
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
            //Bus bus = new Bus( this._busSpawnVector,
            //    new Vector3( this.Player.Character.Position.X, this.Player.Character.Position.Y,
            //        this.Player.Character.Position.Z ) );
            Bus bus = new Bus( this._busSpawnVector, new Vector3( 1235.09F, -89.62F, 28.03F ) );
            this._busses.Add( bus );
            Blip blip = Blip.AddBlip( bus.BusVehicle );
            blip.Color = BlipColor.DarkTurquoise;
            blip.Name = "Bus";
            this._blips.Add( bus, blip );
        }
    }
}
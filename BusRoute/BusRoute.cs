#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            private Vector3 _dest;
            private Blip _blip;

            public Vehicle BusVehicle
            {
                get { return this._bus; }
            }

            public Ped Driver
            {
                get { return this._driver; }
            }

            public Bus( Vector3 spawnPos )
                : this()
            {
                this._bus = World.CreateVehicle( "BUS", spawnPos );
                //this._bus.RotationQuaternion = new Quaternion();
                this._bus.Rotation = new Vector3( 15, 0, 195 );
                this._passagers = new List<Ped>();
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
            /// Создает водителя для автобуса
            /// </summary>
            public Ped CreateDriver()
            {
                this._driver = World.CreatePed( this._bus.Position.Around( 10F ), Gender.Male );
                var tasks = new TaskSequence();
                tasks.AddTask.EnterVehicle( this._bus, VehicleSeat.Driver );
                this._driver.Task.PerformSequence( tasks );
                return this._driver;
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

            public void DriveRoute( Vector3 targetPoint )
            {
                var tasks = new TaskSequence();
                this._dest = targetPoint;
                tasks.AddTask.DriveTo( this._dest, 25, true, true );
                this._driver.Task.PerformSequence( tasks );
                //while ( !this._driver.Position.Equals( _dest.Around( 3F ) ) )
                //{
                //    this.Wait( 1500 );
                //}
                //this._driver.NoLongerNeeded();
                //this._bus.NoLongerNeeded();
                ////this._checkpoint.Disable();
                //Array.ForEach( this._passagers.ToArray(), p => p.NoLongerNeeded() );
                //Game.DisplayText( "Done" );
            }
        }

        //private readonly Vector3 _busSpawnVector = new Vector3( 1039.08F, 280.66F, 30.45F );
        private readonly Vector3 _busSpawnVector = new Vector3( 1039.08F, 280.66F, 30.45F );
        private readonly List<Bus> _busses;
        private readonly Dictionary<Bus, Blip> _blips;

        public BusRoute()
        {
            this.Interval = 100;
            this.Tick += this.BusRoute_Tick;
            this.KeyDown += new GTA.KeyEventHandler( this.BasicKeyExample_KeyDown );
            this._busses = new List<Bus>();
            this._blips = new Dictionary<Bus, Blip>();
        }

        private void BusRoute_Tick( object sender, EventArgs e )
        {
        }

        private void BasicKeyExample_KeyDown( object sender, GTA.KeyEventArgs e )
        {
            if ( e.Key == Keys.F5 )
            {
                this.SpawnBusWithDriver();
            }
        }

        private void WaitWhilePedNotInCar( Ped ped, Vehicle vehicle )
        {
            if ( !ped.isInVehicle( vehicle ) )
            {
                Game.DisplayText( "No in Veh" );
                this.Wait( 1000 );
            }
        }

        private void CreateDriver( Bus bus )
        {
            Ped driver = bus.CreateDriver();
            this.WaitWhilePedNotInCar( driver, bus.BusVehicle );
        }

        private void CreatePassagers( Bus bus, int passagersCount )
        {
            for ( int i = 0; i < passagersCount; i++ )
            {
                Ped passager = bus.CreatePassager();
                this.WaitWhilePedNotInCar( passager, bus.BusVehicle );
            }
        }

        private void SpawnBusWithDriver()
        {
            Bus bus = new Bus( this._busSpawnVector );
            this._busses.Add( bus );
            this.Wait( 3500 );
            this.CreateDriver( bus );
            this.Wait( 10500 );
            this.CreatePassagers( bus, 1 );
            Blip blip = Blip.AddBlip( bus.BusVehicle );
            blip.Color = BlipColor.DarkRed;
            blip.Name = "Bus";
            this._blips.Add( bus, blip );
            bus.DriveRoute( this.Player.Character.Position );
        }
    }
}
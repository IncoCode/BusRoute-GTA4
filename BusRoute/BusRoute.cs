#region Using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using System.Timers;

#endregion

namespace BusRoute
{
    public class BusRoute : Script
    {
        public class Bus : Script
        {
            private readonly Vehicle _bus;
            private Ped _driver;
            private readonly List<Ped> _passagers;
            private Checkpoint _checkpoint;
            private GTA.Timer _timer;
            private Vector3 _dest;
            private Blip _blip;

            public Vehicle BusVehicle { get { return this._bus; } }

            public Bus( Vector3 spawnPos )
                : this()
            {
                this._bus = World.CreateVehicle( "BUS", spawnPos );
                this.Wait( 3000 );
                //this._bus.RotationQuaternion = new Quaternion();
                //this._bus.Rotation = new Vector3( 0, 15, 0 );
                this._passagers = new List<Ped>();
                this.CreateDriver();
            }

            public Bus()
            {
                this.Interval = 1000;
                this.Tick += Bus_Tick;
            }

            private void Bus_Tick( object sender, EventArgs e )
            {
                if ( this._bus == null )
                {
                    Game.DisplayText( "Null", 500 );
                    return;
                }
                this._blip = Blip.AddBlip( this._bus );
                this._blip.Color = BlipColor.DarkRed;
            }

            private static Gender GetRandomGender()
            {
                Random random = new Random();
                return random.Next( 1, 100 ) >= 50 ? Gender.Male : Gender.Female;
            }

            /// <summary>
            /// Создает водителя для автобуса
            /// </summary>
            private void CreateDriver()
            {
                this._driver = World.CreatePed( this._bus.Position.Around( 15F ), Gender.Male );
                var tasks = new TaskSequence();
                tasks.AddTask.EnterVehicle( this._bus, VehicleSeat.Driver );
                this._driver.Task.PerformSequence( tasks );
                this.WaitWhileNoInBus( this._driver );
            }

            /// <summary>
            /// Ждем, пока пед не в автобусе
            /// </summary>
            /// <param name="ped"></param>
            private void WaitWhileNoInBus( Ped ped )
            {
                while ( !ped.isInVehicle( this._bus ) )
                {
                    this.Wait( 1000 );
                }
            }

            /// <summary>
            /// Создает пассажира для автобуса
            /// </summary>
            private void CreatePassager()
            {
                Ped passager = World.CreatePed( this._bus.Position.Around( 20F ), GetRandomGender() );
                var tasks = new TaskSequence();
                tasks.AddTask.EnterVehicle( this._bus, VehicleSeat.AnyPassengerSeat );
                passager.Task.PerformSequence( tasks );
                this._passagers.Add( passager );
                this.WaitWhileNoInBus( passager );
            }

            /// <summary>
            /// Создает указанное к-во пассажиров
            /// </summary>
            /// <param name="passagersCount">К-во пассажиров</param>
            public void CreatePassagers( int passagersCount )
            {
                for ( int i = 0; i < passagersCount; i++ )
                {
                    this.CreatePassager();
                }
            }

            public void DriveRoute()
            {
                var tasks = new TaskSequence();
                if ( this.Player == null )
                {
                    Game.DisplayText( "Player null!" );
                    return;
                }
                _dest = this.Player.Character.Position;
                tasks.AddTask.DriveTo( _dest, 25, true, true );
                this._driver.Task.PerformSequence( tasks );
                while ( !this._driver.Position.Equals( _dest.Around( 3F ) ) )
                {
                    this.Wait( 1500 );
                }
                this._driver.NoLongerNeeded();
                this._bus.NoLongerNeeded();
                //this._checkpoint.Disable();
                Array.ForEach( this._passagers.ToArray(), p => p.NoLongerNeeded() );
                Game.DisplayText( "Done" );
            }
        }

        //private readonly Vector3 _busSpawnVector = new Vector3( 1039.08F, 280.66F, 30.45F );
        private readonly Vector3 _busSpawnVector = new Vector3( 1039.08F, 280.66F, 30.45F );
        private List<Bus> _busses;
        private Blip _blip = null;

        public BusRoute()
        {
            this.Interval = 100;
            this.Tick += BusRoute_Tick;
            this.KeyDown += new GTA.KeyEventHandler( this.BasicKeyExample_KeyDown );
            this._busses = new List<Bus>();
        }

        void BusRoute_Tick( object sender, EventArgs e )
        {
            //Game.DisplayText( "Suka", 500 );
            if ( this._busses.Count == 0 )
            {
                Game.DisplayText( "Nope", 500 );
                return;
            }
            //if ( this._blip == null )
            //{

            //}
            Game.DisplayText( "Yep" );
            //this._blip = Blip.AddBlip( this._busses[ 0 ].BusVehicle.Position );
            //this._blip.Color = BlipColor.DarkRed;
            //            this._blip = new Blip()
            //this._blip.Enabled = true;
        }

        private void BasicKeyExample_KeyDown( object sender, GTA.KeyEventArgs e )
        {
            if ( e.Key == Keys.F5 )
            {
                this.SpawnBusWithDriver();
                //this._blip = Blip.AddBlip( this.Player.Character );
                //this._blip.Color = BlipColor.DarkRed;
            }
        }

        private void SpawnBusWithDriver()
        {
            Bus bus = new Bus( this._busSpawnVector );
            this._busses.Add( bus );
            bus.CreatePassagers( 1 );
            bus.DriveRoute();
        }
    }
}
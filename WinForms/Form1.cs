using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using DevExpress.XtraEditors;
using HuntingDog.DogEngine;

namespace WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();     
            //ucView21.Factory = new SearchObjectFactory();
        }

        FakeStudioController _fakeCtrl = new FakeStudioController();


        private void Form1_Load(object sender, EventArgs e)
        {
            // Fill fake controller with fake data

            _fakeCtrl.EmptyDataBase = "Backup";
            _fakeCtrl.SmallDataBase = "Restoration_from_big_storage";

            _fakeCtrl.OnAction += new Action<string>(_fakeCtrl_OnAction);
            _fakeCtrl.FakeServers = new List<string> { "Ignorance", "Greed", "Arrogance" };
            _fakeCtrl.FakeDatabases = new List<string> { "All Human Sins", "Good Humans", "Backup","Restoration_from_big_storage" };
            _fakeCtrl.FakeFindList = new List<Entity>{ 

                new Entity{IsTable = true, Name = "Special Table1"},
                new Entity{IsTable = true, Name = "ThisYearGoals"},
                new Entity{IsTable = true, Name = "Unfinished"},
                new Entity{IsTable = true, Name = "Wishes"},
                new Entity{IsTable = true, Name = "Prerequisutes"},
                new Entity{IsTable = true, Name = "Dedication"},
                 new Entity{IsTable = true, Name = "ChickenCurry"},


                new Entity{IsProcedure = true, Name = "AddGoal"},
                new Entity{IsProcedure = true, Name = "CreateOrUpdateLastGoal"},
                new Entity{IsProcedure = true, Name = "RemoveGoal"},
                new Entity{IsProcedure = true, Name = "DeleteWishCompletelyFromTable"},
                new Entity{IsProcedure = true, Name = "ListUnfinished"},

                new Entity{IsView = true, Name = "Sophisticated View1"},
                new Entity{IsView = true, Name = "AnotherPoint"},
                new Entity{IsView = true, Name = "CombinedDataFromYear"},


                new Entity{IsFunction = true, Name = "AddWishAndCheck"},
                new Entity{IsFunction = true, Name = "ParseGoalListBeforeAdding"},
                new Entity{IsFunction = true, Name = "CaluclatePoint"},

            };


            _fakeCtrl.DuplicateFakeList();
            _fakeCtrl.DuplicateFakeList();
            _fakeCtrl.DuplicateFakeList();


          // load wpf control
            _wpf = new HuntingDog.ucHost();
            _wpf.DogFace.StudioController = _fakeCtrl;

            _wpf.Dock = DockStyle.Fill;
            this.splitContainer1.Panel1.Controls.Add(_wpf);
           
        }

        
        void _fakeCtrl_OnAction(string obj)
        {
            Invoke(delegate() { memoEdit1.Text += Environment.NewLine + obj; });
        }
        HuntingDog.ucHost _wpf;


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _wpf.DogFace.Stop();
        }

        public void Invoke(System.Windows.Forms.MethodInvoker invoker)
        {
            Invoke((Delegate)invoker);
        }
    
        
    }

    
}

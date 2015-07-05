using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HuntingDog.Config;
using HuntingDog.DogFace;



namespace WinForms
{


  


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


            var cfg = new DogConfig();
            var persistor = new ConfigPersistor();
            var storage  = new UserPreferencesStorage();

         
            cfg.AlterOrCreate = EAlterOrCreate.Create;
            persistor.Persist(cfg, storage);

            var restored = persistor.Restore<DogConfig>(storage);


            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }




}

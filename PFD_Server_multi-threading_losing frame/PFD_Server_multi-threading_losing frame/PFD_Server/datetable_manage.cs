using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HST_Server
{
   public class Datetable_manage
    {
        public List<string> db_table = new List<string>();
        public Datetable_manage()
        {
            db_table.Add("pflow_tb_data");
            db_table.Add("pflow_tb_data_day");
            db_table.Add("pflow_tb_data_hor");
            db_table.Add("pflow_tb_data_month");
            db_table.Add("pflow_tb_data_week");
            db_table.Add("pflow_tb_data_year");
        }
   }

   public class Datetable_manage2
   {
       public List<string> db_table2 = new List<string>();
       public Datetable_manage2()
       {
           for (int i = 1; i < 22; i++)
           {
               db_table2.Add("basicdata_" + i.ToString());
               db_table2.Add("fivemindata_" + i.ToString());
               db_table2.Add("onedaydata_" + i.ToString());
           }
       }
   }
    public class CreatTable
    {
        public void creat_table()
        {
            MysqlPersistance mp = new MysqlPersistance();
            Datetable_manage tab = new Datetable_manage();
            string table_name = "";
            DateTime creat_tb_time = DateTime.Now.Date;
            for (int i = 0; i < tab.db_table.Count; i++)
            {
                if (mp.check_tb(tab.db_table[i]) > 5000000)
                {
                    table_name = tab.db_table[i] + "_" + creat_tb_time.ToString("yyyyMMdd");
                    mp.create_tb(table_name);
                    mp.copyTB2(tab.db_table[i],table_name);
                }
            }
        }

        public void creat_table2()
        {
            MysqlPersistance mp = new MysqlPersistance();
            Datetable_manage2 tab2 = new Datetable_manage2();
            string table_name = "";
            DateTime creat_tb_time = DateTime.Now.Date;
            for (int i = 0; i < tab2.db_table2.Count; i++)
            {
                if (mp.check_tb(tab2.db_table2[i]) > 5000000)
                {
                    table_name = tab2.db_table2[i] + "_" + creat_tb_time.ToString("yyyyMMdd");
                    mp.create_basic_tb(table_name);
                    mp.copyTB(tab2.db_table2[i], table_name);
                }
            }
        }
    }
    
}

﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using U8.Interface.Bus;
using U8.Interface.Bus.Comm;
using U8.Interface.Bus.ApiService;

using U8.Interface.Bus.ApiService.Model;
using U8.Interface.Bus.ApiService.BLL;
using U8.Interface.Bus.ApiService.DAL;
using U8.Interface.Bus.DBUtility;


namespace U8.Interface.Bus.ApiService.Voucher.Factory.CQ
{
    /// <summary>
    /// 材料出库单(HY_DZ_K7_DLLReflect预置的op类)
    /// </summary>
    public class RdRecord11 : StockOP
    {
        private int tasktype = 0;

        /// <summary>
        /// 中间表
        /// </summary>
        private string cardNo = "0412";
        private string headtable = "MES_CQ_rdrecord11";
        private string bodytable = "mes_cq_rdrecords11"; 
        private string voucherNoColumnName = "cRdCode"; 

        private string sourceCardNo = "MO21";
        private string sourceHeadTable = "v_mom_order_wf";
        private string sourceBodyTable = "v_mom_orderdetail_wf";  //mom_moallocate


        private string taskStatusflagColName = "operflag";

        public override string SetTableName()
        {
            return "rdrecord11";
        }

        public override string SetVouchType()
        {
            return "11";
        }

        public override string SetApiAddressAdd()
        {
            return "U8API/MaterialOut/Add";
        }

        public override string SetApiAddressAudit()
        {
            return "U8API/MaterialOut/Audit";
        }

        public override string SetApiAddressCancelAudit()
        {
            return "U8API/MaterialOut/CancelAudit";
        }

        public override string SetApiAddressDelete()
        {
            return "U8API/MaterialOut/Delete";
        }

        public override string SetApiAddressLoad()
        {
            return "U8API/MaterialOut/Load";
        }

        public override string SetApiAddressUpdate()
        {
            return "U8API/MaterialOut/Update";
        }

        public override TaskList GetTask()
        {
            //string sql = "SELECT * FROM " + headtable + " WHERE operflag = 0 ";
            string sql = " SELECT distinct T.* FROM " + headtable + " T WITH(NOLOCK) ";
            sql += " INNER JOIN " + bodytable + " B WITH(NOLOCK) ON T.id = B.id  ";
            sql += " INNER JOIN MES_CQ_mom_order MT WITH(NOLOCK) ON MT.MoCode = B.MoCode  AND MT.operflag = 1  and T.opertype = MT.opertype and T.operflag = 0 ";

             

            string curid = "";  
            DataTable dt = new DataTable();
            dt = DbHelperSQL.Query(sql).Tables[0];
            if (null == dt || dt.Rows.Count == 0)
            {
                return null;
            }
            TaskList tl = new TaskList();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Task t = new Task();
                VoucherType v = new VoucherType();
                v.SourceCardNo = sourceCardNo;
                //v.SourceVoucherNo = dt.Rows[i][""].ToString();
                v.CardNo = cardNo;
                v.VoucherName = "材料出库单";
                t.VouchType = v;
                t.taskType = 0;   //MES接口任务  
                t.OperType = (int)dt.Rows[i]["OperType"];
                try
                {
                    t.id =  dt.Rows[i]["id"].ToString();  //int.Parse(dt.Rows[i]["id"].ToString());
                }
                catch (Exception ee)
                {
                    ApiService.BLL.Common.ErrorMsg(SysInfo.productName, "id 值出错！");
                }
                try
                {
                    t.OperType = int.Parse(dt.Rows[i]["opertype"].ToString());
                }
                catch (Exception ee)
                {
                    ApiService.BLL.Common.ErrorMsg(SysInfo.productName, "opertype 值出错！");
                }
                tl.Add(t);
                curid += "'" + t.id + "',";
                break;
            }
            if (!string.IsNullOrEmpty(curid))
            {
                curid = curid.Substring(0, curid.Length - 1);
                string[] ids = curid.Split(',');
                string msql = " update " + headtable + " set " + taskStatusflagColName + " = 2 where id in (" + ids[0] + ") ";
                DbHelperSQL.ExecuteSql(msql);
            }
            return tl;
        }



        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        /// <param name="autoid"> ID</param>
        /// <returns></returns>
        public override Model.Synergismlog GetLogModel(string autoid)
        {
            Model.Synergismlog tmpd = new Synergismlog(); 
            tmpd.Id = autoid;
            tmpd.Cvouchertype = cardNo; 
            tmpd.Cstatus = U8.Interface.Bus.ApiService.DAL.Constant.SynerginsLog_Cstatus_NoDeal;
            //DataSet ds = DbHelperSQL.Query("SELECT b.cRdCode,b.id,b.did,t.opertype FROM " + bodytable + " b with(nolock) left join " + headtable + " t with(nolock) on b.id = t.id WHERE b.DID = " + autoid);
            DataSet ds = DbHelperSQL.Query("SELECT t.cRdCode,t.id,t.opertype FROM " + headtable + " t with(nolock)  WHERE t.id = '" + autoid + "' ");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                tmpd.Cvoucherno = ds.Tables[0].Rows[i]["cRdCode"].ToString();
                tmpd.Id = ds.Tables[0].Rows[i]["id"].ToString(); // int.Parse(ds.Tables[0].Rows[i]["id"].ToString());
                //tmpd.Cdealmothed = int.Parse(ds.Tables[0].Rows[i]["opertype"].ToString()) + 1; // 0(自动生单/自动审核) 1增 2修改 3删
            }
            return tmpd;
        }


        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        /// <param name="autoid">子任务ID</param>
        /// <returns></returns>
        public override Model.Synergismlogdt GetModel(string autoid)
        {
            Model.Synergismlogdt tmpd =  new Model.Synergismlogdt();
            tmpd.Autoid = autoid;
            tmpd.Id = autoid;
            tmpd.Cvouchertype = cardNo;
            tmpd.Ilineno = 2;
            tmpd.Cstatus = U8.Interface.Bus.ApiService.DAL.Constant.SynerginsLog_Cstatus_NoDeal;
            DataSet ds = DbHelperSQL.Query("SELECT t.cRdCode,t.id,t.opertype FROM " + headtable + " t with(nolock)  WHERE t.id = " + U8.Interface.Bus.Comm.Convert.ConvertDbValueFromPro(autoid,"string"));
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                tmpd.Cvoucherno = ds.Tables[0].Rows[i]["cRdCode"].ToString();
                tmpd.Autoid = ds.Tables[0].Rows[i]["id"].ToString(); // int.Parse(ds.Tables[0].Rows[i]["id"].ToString());
                tmpd.Id =  ds.Tables[0].Rows[i]["id"].ToString();
                tmpd.Cdealmothed = int.Parse(ds.Tables[0].Rows[i]["opertype"].ToString()) + 1; // 0(自动生单/自动审核) 1增 2修改 3删
            }
            return tmpd;
        }



        public override Synergismlogdt GetFirst(Synergismlogdt dt)
        {
            Model.Synergismlogdt fdt = new Model.Synergismlogdt();
            fdt.Cvouchertype = sourceCardNo;
            fdt.Id = dt.Id;
            fdt.Ilineno = 1;
            DataSet ds = DbHelperSQL.Query("SELECT MoCode FROM " + bodytable + " with(nolock) WHERE ID = " + U8.Interface.Bus.Comm.Convert.ConvertDbValueFromPro(dt.Id,"string"));
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                fdt.Cvoucherno = ds.Tables[0].Rows[i]["MoCode"].ToString();
                fdt.Autoid = dt.Id;
            }
            return fdt;

        }



        /// <summary>
        /// 获取上一结点
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override Model.Synergismlogdt GetPrevious(Model.Synergismlogdt dt)
        {
            if (dt.Cvouchertype == cardNo)
            {
                Model.Synergismlogdt pdt = new Model.Synergismlogdt();
                pdt.Cvouchertype = sourceCardNo;
                pdt.Id = dt.Id;
                DataSet ds = DbHelperSQL.Query("SELECT MoCode FROM " + bodytable + " with(nolock) WHERE ID = " + U8.Interface.Bus.Comm.Convert.ConvertDbValueFromPro(dt.Id,"string"));
                for (int i = 0; i < ds.Tables[0].Rows.Count;i++ )
                {
                    pdt.Cvoucherno = ds.Tables[0].Rows[i]["MoCode"].ToString();
                    //pdt.Autoid = null;
                } 
                return pdt;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 获取下一任务结点
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override List<Model.Synergismlogdt> GetNext(Model.Synergismlogdt dt)
        { 
            List<Model.Synergismlogdt> logdt = new List<U8.Interface.Bus.ApiService.Model.Synergismlogdt>();
            if (dt.Ilineno == 1)
            {
                Model.Synergismlogdt tmpd =  new Model.Synergismlogdt();
                tmpd.Id = dt.Id;
                tmpd.Cvouchertype = cardNo;
                tmpd.Ilineno = 2;
                tmpd.TaskType = tasktype;
                tmpd.Cstatus = U8.Interface.Bus.ApiService.DAL.Constant.SynerginsLog_Cstatus_NoDeal;
                tmpd.Isaudit = U8.Interface.Bus.ApiService.DAL.Constant.SynergisnLogDT_Isaudit_True;

                DataSet ds = DbHelperSQL.Query("SELECT t.cRdCode,t.id,t.opertype FROM " + headtable + " t with(nolock)  WHERE t.id = " + U8.Interface.Bus.Comm.Convert.ConvertDbValueFromPro(dt.Id,"string"));
           
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    tmpd.Cvoucherno = ds.Tables[0].Rows[i]["cRdCode"].ToString();
                    tmpd.Autoid =  ds.Tables[0].Rows[i]["id"].ToString(); // int.Parse(ds.Tables[0].Rows[i]["id"].ToString());
                    tmpd.Cdealmothed = int.Parse(ds.Tables[0].Rows[i]["opertype"].ToString()) + 1; // 0(自动生单/自动审核) 1增 2修改 3删
                } 
                
                logdt.Add(tmpd);
                return logdt;
            }
            else
            {
                return logdt;
            }        
        }



        /// <summary>
        /// 获取来源表头数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pdt"></param>
        /// <param name="apidata"></param>
        /// <returns></returns>
        public override System.Data.DataSet SetFromTabet(Model.Synergismlogdt dt, Model.Synergismlogdt pdt, Model.APIData apidata)
        { 

            ApiService.DAL.TaskLog.ITaskLogDetail dtdal = ClassFactory.GetITaskLogDetailDAL(apidata.TaskType);
            Model.ConnectInfo cimodel = dtdal.GetConnectInfo(pdt);

            string sql = "select t.*,";
            sql += "lt.cWhCode as MES_cWhCode ,lt.cRdCode as cCode,lt.cvencode as mes_cvencode, lt.cDepCode as mes_cdepcode,lt.cRdStyleCode as MES_cRdStyleCode ";
            sql += ",'" + System.DateTime.Now.ToString(SysInfo.dateFormat) + "' as ddate,";
            sql += " lb.MoCode as MES_T_MOCODE ";
            sql += ",'生产订单' as cSource ";
            sql += " from  " + sourceHeadTable + " t with(nolock) INNER JOIN " + bodytable + " lb with(nolock) on lb.mocode = t.mocode INNER JOIN " + headtable + " lt with(nolock) on lt.id = lb.id where lt.id ='" + pdt.Id + "' ";
            DbHelperSQLP help = new DbHelperSQLP(cimodel.Constring);
            DataSet ds = help.Query(sql);
            ApiService.BLL.Common.ErrorMsg(ds, "未能获取生产订单表头信息");
            return ds;
 
        }

  
        /// <summary>
        /// 获取来源表体数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pdt"></param>
        /// <param name="apidata"></param>
        /// <returns></returns>
        public override System.Data.DataSet SetFromTabets(Model.Synergismlogdt dt, Model.Synergismlogdt pdt, Model.APIData apidata)
        {
            ApiService.DAL.TaskLog.ITaskLogDetail dtdal = ClassFactory.GetITaskLogDetailDAL(apidata.TaskType);  //ClassFactory.GetITaskLogMain(3);;
            Model.ConnectInfo cimodel = dtdal.GetConnectInfo(pdt);
            string sql = "select b.*,";
            sql += " sb.DSortSeq as DSortSeq , ";
            sql += " lt.cWhCode as MES_cWhCode,lt.cRdCode as cCode,sb.modid as modid,"; 
            sql += " lb.iquantity as MES_iquantity, lb.cVenCode as mes_cvencode,lb.cpinvcode as mes_cpinvcode,lb.cInvCode as mes_cinvcode,lb.mocode as mes_mocode,  "; 
            sql += " CASE lb.opertype WHEN 0 THEN 'A' WHEN 1 THEN 'M' WHEN '2' THEN 'D' ELSE 'A' END as editprop,  ";
            sql += " lb.MOCODE as MES_B_MOCODE ";
            sql += " from " + sourceBodyTable + " sb with(nolock) INNER JOIN  " + sourceHeadTable + " t with(nolock) on sb.moid = t.moid ";
            sql += " INNER JOIN mom_moallocate b with(nolock) on b.modid = sb.modid ";
            sql += " INNER JOIN " + bodytable + " lb with(nolock) on lb.mocode = t.mocode and lb.cinvcode = b.invcode and lb.cpinvcode = sb.dinvcode "; 
            sql += " INNER JOIN " + headtable + " lt with(nolock) on lt.id = lb.id where lt.id ='" + pdt.Id + "' ";
            
            DbHelperSQLP help = new DbHelperSQLP(cimodel.Constring);
            DataSet ds = help.Query(sql);
            ApiService.BLL.Common.ErrorMsg(ds, "未能获取生产订单表体信息");
            return ds;
        }

         

        /// <summary>
        /// ID CODE 互查
        /// </summary>
        /// <param name="strID"></param>
        /// <param name="bd"></param>
        /// <param name="codeorid"></param>
        /// <returns></returns>
        public override string GetCodeorID(string strID, BaseData bd, string codeorid)
        {
            string sqlstr = string.Empty;
            if (codeorid == "id")
            {
                sqlstr = "select isnull(id,'') from rdrecord11  with(nolock)  where ccode='" + strID + "'";
            }
            if (codeorid == "code")
            {
                sqlstr = "select isnull(ccode,'') from rdrecord11  with(nolock)  where id='" + strID + "'";
            }
            Model.APIData apidata = bd as Model.APIData;

            DBUtility.DbHelperSQLP sqlp = new DBUtility.DbHelperSQLP(apidata.ConnectInfo.Constring);
            string ret = sqlp.GetSingle(sqlstr).NullToString();
            ApiService.BLL.Common.ErrorMsg(ret, "未能获材料出库单ID或单号");
            return ret;
        }

        public override bool CheckAuditStatus(string strVoucherNo, string strConn)
        {
            return false;
        }

        #region update  log


        public override int Update(Model.Synergismlog dt)
        { 
            return CQ.Utility.UpdateMainLog(dt, headtable, voucherNoColumnName, taskStatusflagColName, "cerrordesc"); 
        }



        //修改日志
        public override int Update(Model.Synergismlogdt dt)
        {
            return CQ.Utility.UpdateDetailLog(dt, headtable, voucherNoColumnName, taskStatusflagColName, "cerrordesc");
             
        }


        #endregion

         


    }
}

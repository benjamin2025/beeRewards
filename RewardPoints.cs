using System;
using System.Data;

public class RewardPoints{

    
    struct Promotions{
        public int Id{get;set;}
        public decimal ObjectiveAmount{get;set;}
        public decimal CapAmount{get;set;}
        public int AwardPoints{get;set;}
        public DateTime PromoStartDate{get;set;}
        public DateTime PromoEndDate{get;set;}

        public Promotions(int id, decimal objectiveAmount, decimal capAmount, int awardPoints, DateTime promoStartDate, DateTime promoEndDate){
            Id = id;
            ObjectiveAmount = objectiveAmount;
            CapAmount = capAmount;
            AwardPoints = awardPoints;
            PromoStartDate = promoStartDate;
            PromoEndDate = promoEndDate;
        }
    }

    private DataSet dtSet;

    delegate int PromoRewardPoints(decimal transAmount, Promotions promo);

    public RewardPoints(){
        ProcessTransactions();
    }

    private void ProcessTransactions(){

        // assuming transaction data has been sorted
        CreateTransactionsTable();

        DataTable dt = new DataTable();
        dt = dtSet.Tables["Transactions"];

        var transCount = dt.Rows.Count;
        DateTime dtTrans;
        decimal transAmt;
        int rewardPoints;

        Promotions promoFifty = new Promotions(1, 50.00M, 100.00M, 1, DateTime.Parse("01/01/2020 00:00:00.001"), DateTime.Parse("03/31/2020 23:59:59.999"));
        Promotions promoOneHundred = new Promotions(1, 100.00M, 0M, 2, DateTime.Parse("01/01/2020 00:00:00.001"), DateTime.Parse("03/31/2020 23:59:59.999"));

        PromoRewardPoints prp1, prp2;
        prp1 = PointsEarnedAboveFifty;
        prp2 = PointsEarnedAboveOneHundred;

        int monthlyRewardPoints = 0;
        int totalRewardPoints = 0;
        int prevTransMonth = 0;
        int prevTransYear = 0;

        for(int x = 0; x < transCount; x++)
        {
            dtTrans = (DateTime)dt.Rows[x]["transDate"];
            transAmt = (decimal)dt.Rows[x]["transAmount"];
            rewardPoints = 0;

            // promo 1
            if(!IsPromoExpired(dtTrans, promoFifty)){
                if (!IsAmountBelowIncentive(transAmt, promoFifty)){
                    rewardPoints = prp1(transAmt, promoFifty);
                }
            }

            // promo 2
            if(!IsPromoExpired(dtTrans, promoOneHundred)){
                rewardPoints += prp2(transAmt, promoOneHundred);
            }
            
            if(dtTrans.Month == prevTransMonth && dtTrans.Year == prevTransYear){
                monthlyRewardPoints += rewardPoints;
            } else {
                monthlyRewardPoints = rewardPoints;
            }
            totalRewardPoints += rewardPoints;
            prevTransMonth = dtTrans.Month;
            prevTransYear = dtTrans.Year;

            // test
            DisplayText(dtTrans, transAmt, rewardPoints, monthlyRewardPoints, totalRewardPoints);
        }
    }
    private bool IsPromoExpired(DateTime dtTrans, Promotions promo){
        bool bPromoExpired=true;
        if(dtTrans >= promo.PromoStartDate && dtTrans <= promo.PromoEndDate){
            bPromoExpired = false;
        }
        return bPromoExpired;
    }
    private bool IsAmountBelowIncentive(decimal transAmount, Promotions promo){
        bool bAmountBelowIncentive=false;
        // $50 and under
        if (transAmount <= promo.ObjectiveAmount) {
            bAmountBelowIncentive = true;
        }
        return bAmountBelowIncentive;
    }
    private int PointsEarnedAboveFifty(decimal transAmount, Promotions promo){
        // between $51 and $100
        const int EarnedPoints = 50;
        decimal TransAmountRoundUp;
        TransAmountRoundUp = Math.Ceiling(transAmount);
        int qualifyAmount = 0;
        if (transAmount <= promo.CapAmount) {
            qualifyAmount = (int)(TransAmountRoundUp - promo.ObjectiveAmount) * promo.AwardPoints;
        } else {
            // above $100
            qualifyAmount = EarnedPoints;
        }
        return qualifyAmount;
    }

    private bool IsAmountAboveOneHundred(decimal transAmount, Promotions promo){
        bool amountAboveOneHundred=false;        
        // $100 and under
        if (transAmount > promo.ObjectiveAmount) {
            amountAboveOneHundred = true;
        }
        return amountAboveOneHundred;
    }

    private int PointsEarnedAboveOneHundred(decimal transAmount, Promotions promo){
        // over $100
        decimal TransAmountRoundUp;
        TransAmountRoundUp = Math.Ceiling(transAmount);

        int qualifyAmount = 0;
        if (transAmount > promo.ObjectiveAmount) {
            qualifyAmount = (int)(TransAmountRoundUp - promo.ObjectiveAmount) * promo.AwardPoints;
        }
        return qualifyAmount;
    }

    private int PointsEarned(decimal transAmount, Promotions promo){
        return 0;
    }
    public void DisplayText(DateTime TransDate, decimal TransAmount, int RewardPoints, int monthlyRewardPoints, int totalRewardPoints){
        Console.WriteLine($"Transaction Date is {TransDate}; Amount ${TransAmount}; Reward Points {RewardPoints}; Monthly Points {monthlyRewardPoints}; Total Points {totalRewardPoints}");        
    }
    
    private void CreateTransactionsTable() {
        DataTable TransTable = new DataTable("Transactions");
        DataColumn dtCol;
        DataRow dtRow;

        dtCol = new DataColumn();
        dtCol.DataType = typeof(Int32);
        dtCol.ColumnName = "id";
        dtCol.Caption = "Trans Id";
        dtCol.ReadOnly = false;
        dtCol.Unique = true;
        TransTable.Columns.Add(dtCol);

        dtCol = new DataColumn();
        dtCol.DataType = typeof(DateTime);
        dtCol.ColumnName = "transDate";
        dtCol.Caption = "Trans Date";
        TransTable.Columns.Add(dtCol);

        dtCol = new DataColumn();
        dtCol.DataType = typeof(decimal);
        dtCol.ColumnName = "transAmount";
        dtCol.Caption = "Trans Amount";
        TransTable.Columns.Add(dtCol);

        DataColumn[] PrimaryKeyColumns = new DataColumn[1];
        PrimaryKeyColumns[0] = TransTable.Columns["id"];
        TransTable.PrimaryKey = PrimaryKeyColumns;

        dtSet = new DataSet();
        dtSet.Tables.Add(TransTable);

        // record 1
        dtRow = TransTable.NewRow();
        dtRow["id"] = 101;
        dtRow["transDate"] = DateTime.Parse("01/01/2020 11:00:05.009");
        dtRow["transAmount"] = 120.00M;
        TransTable.Rows.Add(dtRow);

        // record 2
        dtRow = TransTable.NewRow();
        dtRow["id"] = 102;
        dtRow["transDate"] = DateTime.Parse("01/21/2020 13:00:33.001");
        dtRow["transAmount"] = 99.51M;
        TransTable.Rows.Add(dtRow);

        // record 3
        dtRow = TransTable.NewRow();
        dtRow["id"] = 103;
        dtRow["transDate"] = DateTime.Parse("02/03/2020 14:02:22.003");
        dtRow["transAmount"] = 100.49M;
        TransTable.Rows.Add(dtRow);

        // record 4
        dtRow = TransTable.NewRow();
        dtRow["id"] = 104;
        dtRow["transDate"] = DateTime.Parse("03/02/2020 15:15:00.112");
        dtRow["transAmount"] = 50.01M;
        TransTable.Rows.Add(dtRow);

        // record 5
        dtRow = TransTable.NewRow();
        dtRow["id"] = 105;
        dtRow["transDate"] = DateTime.Parse("03/03/2020 18:18:23.118");
        dtRow["transAmount"] = 49.51M;
        TransTable.Rows.Add(dtRow);

        // record 6
        dtRow = TransTable.NewRow();
        dtRow["id"] = 106;
        dtRow["transDate"] = DateTime.Parse("03/31/2020 23:59:59.999");
        dtRow["transAmount"] = 89.50M;
        TransTable.Rows.Add(dtRow);

        // record 7
        dtRow = TransTable.NewRow();
        dtRow["id"] = 107;
        dtRow["transDate"] = DateTime.Parse("04/01/2020 00:00:00.001");
        dtRow["transAmount"] = 59.30M;
        TransTable.Rows.Add(dtRow);

        // record 8
        dtRow = TransTable.NewRow();
        dtRow["id"] = 108;
        dtRow["transDate"] = DateTime.Parse("04/03/2020 15:02:00.123");
        dtRow["transAmount"] = 109.30M;
        TransTable.Rows.Add(dtRow);

        TransTable.AcceptChanges();
    }
}
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Program
{
	public static void Main()
	{
		string contents = @"一、評價司，掌評定物價上下，分買支給等事。每司遣大夫一員、紅帕三人──餘雜差等二十人主一司。其朝夕供應奔走，別有庫官等為之。
　　國王日以宮前瑞泉供客，每日清晨汲入綠木筩二石餘，以鎖鎖之；走十里，送至館中。紅帕秀才九人，分日押送。
　　每日供應米五升，麵四斤，醬、醬油、醋、鹽、菜油各四盞，豆腐三斤，燒酒一瓶，魚、肉各三斤，羊肉二斤，乾魚四斤，雞二，蛋十枚，海蟳二，西瓜二，冬瓜十斤，菜一斤，燭四枝，炭十斤，柴四束。
　　起居日，餽生豬、羊各一，雞二，蛋、魚、海蛇、海蟳、石魚巨、車螯、麵條、麵粉、醬越、醋蒜、胡椒、甘蔗、蕉果(冬易以橘)，燒餅、佳蘇魚各一盤，燒酒一埕，炭一包，燭一束；朔、望，加吉果、米肌、銀酒、黃酒之餽(吉果，以米粉為之，形如薄餅；米肌，如白酒而稍淡；銀酒，即燒酒；黃酒，國中所醞煮，酒色黑醲，少有油氣)。
　　守備、千總，日米四升，醬油、醋、鹽、菜油、米醬各一盞，豬肉三斤，羊肉一斤，生魚二斤，乾魚三斤，雞一，蛋十枚，蔬菜一斤，豆腐一斤，燒酒六盞，小燭二枝，炭五斤，柴二束。全廩給，日米三升，醋、鹽、菜油、豆醬各一盞，豬肉二斤，生魚二斤，乾魚二斤，雞一，蛋五枚，蔬菜一斤，豆腐一斤，燒酒三盞，小燭二枝，柴二束。半廩給，日米二升，醋、鹽、菜油、豆醬各一盞，豬肉一斤，乾魚一斤，雞一，蔬菜一";

		string Keyword = "海蟳,西瓜,冬瓜,蔬菜"; 
			
		Console.WriteLine(getSummaryText(contents, Keyword));
		
	}
	
	/// <summary>
    /// 內容文字 擷取
    /// </summary>
    /// <param name="contents">全文</param>
    /// <param name="Keyword">關鍵字(多值 ,分隔)</param>
    /// <returns>reStr</returns>    
	private static string getSummaryText(string contents, string Keyword)
    {
        int CONTEXT_LENGTH = 5; //取偶數 要取一半
        
        string reStr = "";
        try
        {
			if(string.IsNullOrEmpty(contents)) return reStr; //基本上Input 不會導致有這行，基本上可省略
			
            if (!string.IsNullOrEmpty(Keyword))
            {
                contents = Regex.Replace(contents, "<.*?>", string.Empty); // 清空所有 html tag
                contents = Regex.Replace(contents, "@@@@@@", "\n\r"); // 換行符號
                
				// Deal with keywords about 'words'
				List<string> newKeyList = new List<string>();
				foreach(string key in Keyword.Split(',')){
					if(key.Length >1)
						newKeyList.Add(key);
				}
				
				// Find all of keyword position in contents
				List<string> keyIdxList = new List<string>();
				foreach(string key in newKeyList){
					int idx = 0;
					while(contents.IndexOf(key, idx) > -1){
						string idxStr = string.Format("{0}-{1}",contents.IndexOf(key, idx), contents.IndexOf(key, idx)+key.Length );
						keyIdxList.Add(idxStr);
						
						// update idx
						idx = contents.IndexOf(key, idx)+key.Length;
					}					
				}				
				keyIdxList.Sort();
				
				// Important!!!
				// 整合可能重疊的  may fix
				string newIdxStr = "";
				List<string> newKeyIdxList = new List<string>();
				for(int i=0; i<keyIdxList.Count; i++){
					string currentIdxStr = keyIdxList[i];
					// compare next idx
					// 和下一個比較
					for(int j=i+1; j<keyIdxList.Count; j++){
						// // next idx
						// string eIdx = keyIdxList[j];
						
						if(Convert.ToInt32(keyIdxList[j].Split('-')[0])
						- Convert.ToInt32(keyIdxList[i].Split('-')[1])
						< CONTEXT_LENGTH)
						{
							//合併 i~i+1
							newIdxStr = string.Format("{0}-{1}",
											currentIdxStr.Split('-')[0],
											keyIdxList[j].Split('-')[1]);
							i++;
						}
						else{
							break;
						}
					}
					
					if(!string.IsNullOrEmpty(newIdxStr)){
						newIdxStr  = string.Format("{0}-{1}",
											currentIdxStr.Split('-')[0],
											newIdxStr.Split('-')[1]);
					}
					else{
						newIdxStr = currentIdxStr;
					}
					newKeyIdxList.Add(newIdxStr);
					newIdxStr = "";
				}
				
				foreach(string ele in newKeyIdxList)
				{
					int sIdx = Convert.ToInt32(ele.Split('-')[0]);
					int eIdx = Convert.ToInt32(ele.Split('-')[1]);
					
					// pre + 5
					sIdx = sIdx > CONTEXT_LENGTH ? sIdx-CONTEXT_LENGTH : 0;
					// post + 5
					eIdx = eIdx + CONTEXT_LENGTH < contents.Length? eIdx + CONTEXT_LENGTH:contents.Length;
					reStr += string.Format("...{0}...",contents.Substring(sIdx, eIdx-sIdx));
				}
                
				// mark
                foreach(string key in newKeyList){
					reStr = reStr.Replace(key, string.Format("<font class='inverse'>{0}</font>", key));
				}
            }
        }
        catch (Exception ex)
        {
           throw ex;
        }
        return reStr;
    }
}
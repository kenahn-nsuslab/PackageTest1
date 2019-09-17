using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Bebop.Protocol;
using System;

using Common.Scripts.Localization;

namespace  Bebop.UI
{

    
    public class CardRankHelper
    {

        private static string RankStringFormat = "{0} {1}";

        /// <summary>
        /// rank 값과 하이카드 값으로 포커 족보 스트링을 리턴한다.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="highCards">족보의 하이카드</param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetRankString(RankCode code , List<string> highCards , string lang="en")
        {

            //string highCardString = highCards.Count > 1?  string.Join(",", highCards):highCards[0];

            //return string.Format(RankStringFormat , highCardString , GetRankTitleByCode(code));
            //TODO: 카드 랭크 스트링이 스트레이트 나 풀하우스 에서 정확하지 않아 일단 약식으로..
            return GetRankTitleByCode(code);
        }


        // public static string GetRankTitleByRankValue(int rank)
        // {
        //     if (rank >= (int) RankCode.HC) return GetRankTitleByCode(RankCode.HC);
        //     else if ( rank >= (int) RankCode.OP) return GetRankTitleByCode(RankCode.OP);
        //     else if ( rank >= (int) RankCode.TP) return GetRankTitleByCode(RankCode.TP);
        //     else if ( rank >= (int) RankCode.TC) return GetRankTitleByCode(RankCode.TC);
        //     else if ( rank >= (int) RankCode.S) return GetRankTitleByCode(RankCode.S);
        //     else if ( rank >= (int) RankCode.F) return GetRankTitleByCode(RankCode.F);
        //     else if ( rank >= (int) RankCode.FH) return GetRankTitleByCode(RankCode.FH);
        //     else if ( rank >= (int) RankCode.FC) return GetRankTitleByCode(RankCode.FC);
        //     else if ( rank >= (int) RankCode.SF) return GetRankTitleByCode(RankCode.SF);
        //     else
        //     {
        //         throw new ArgumentOutOfRangeException("rank("+rank+") is out of range");
        //     }
        // }


        public static string GetRankTitleByCode(RankCode code)
        {
            //다국어 지원
            //Common.Scripts.Localization.LocalizationManager.Instance.GetText()

            string ret="";

            switch(code)
            {
                case RankCode.HC :

                    //ret = "High Card";
                    ret = LocalizationManager.Instance.GetText("rank_high");
                    break;

                case RankCode.OP :

                    //ret ="One Pair";
                    ret = LocalizationManager.Instance.GetText("rank_1pair");
                    break;
                
                case RankCode.TP :

                    //ret ="Two Pairs";
                     ret = LocalizationManager.Instance.GetText("rank_2pair");
                    break;

                case RankCode.TC :
                    //ret = "Three Cards";
                     ret = LocalizationManager.Instance.GetText("rank_three");
                    break;

                case RankCode.S :

                    //ret ="Straight";
                    ret = LocalizationManager.Instance.GetText("rank_straight");

                    break;

                case RankCode.F :

                    //ret= "Flush";
                     ret = LocalizationManager.Instance.GetText("rank_flush");

                    break;
                
                case RankCode.FH:

                    //ret ="Full House";
                     ret = LocalizationManager.Instance.GetText("rank_fullhouse");

                    break;

                case RankCode.FC :


                    //ret = "Four Cards";
                     ret = LocalizationManager.Instance.GetText("rank_four");

                    break;

                case RankCode.SF:

                    //ret = "Straight Flush";
                     ret = LocalizationManager.Instance.GetText("rank_stf");

                    break;


            }

            return ret;
        }

        public static string GetRankTitleKeyByCode(RankCode code)
        {
            string ret = "";

            switch (code)
            {
                case RankCode.HC:

                    ret = "rank_high";
                    //ret = LocalizationManager.Instance.GetText("rank_high");
                    break;

                case RankCode.OP:

                    ret ="rank_1pair";
                    //ret = LocalizationManager.Instance.GetText("rank_1pair");
                    break;

                case RankCode.TP:

                    ret ="rank_2pair";
                    //ret = LocalizationManager.Instance.GetText("rank_2pair");
                    break;

                case RankCode.TC:
                    ret = "rank_three";
                    //ret = LocalizationManager.Instance.GetText("rank_three");
                    break;

                case RankCode.S:

                    ret = "rank_straight";
                    //ret = LocalizationManager.Instance.GetText("rank_straight");

                    break;

                case RankCode.F:

                    ret= "rank_flush";
                    //ret = LocalizationManager.Instance.GetText("rank_flush");

                    break;

                case RankCode.FH:

                    ret ="rank_fullhouse";
                    //ret = LocalizationManager.Instance.GetText("rank_fullhouse");

                    break;

                case RankCode.FC:
                    ret = "rank_four";
                    //ret = LocalizationManager.Instance.GetText("rank_four");

                    break;

                case RankCode.SF:

                    ret = "rank_stf";
                    //ret = LocalizationManager.Instance.GetText("rank_stf");
                    break;
            }

            return ret;
        }



    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.Record.Formula.Eval;
using System.Text.RegularExpressions;
using NPOI.SS.Util;

namespace NPOI.HSSF.Record.Formula.Functions.Text
{
    public class Text : Fixed2ArgFunction
    {

        public override ValueEval Evaluate(int srcRowIndex, int srcColumnIndex, ValueEval arg0, ValueEval arg1)
        {
            double s0;
            String s1;
            try
            {
                s0 = TextFunction.EvaluateDoubleArg(arg0, srcRowIndex, srcColumnIndex);
                s1 = TextFunction.EvaluateStringArg(arg1, srcRowIndex, srcColumnIndex);
            }
            catch (EvaluationException e)
            {
                return e.GetErrorEval();
            }
            if (Regex.Match(s1, @"[\d,\#,\.,\$,\,]+").Success)
            {
                FormatBase formatter = new DecimalFormat(s1);
                return new StringEval(formatter.Format(s0));
            }
            else if (s1.IndexOf("/") == s1.LastIndexOf("/") && s1.IndexOf("/") >= 0 && !s1.Contains("-"))
            {
                double wholePart = Math.Floor(s0);
                double decPart = s0 - wholePart;
                if (wholePart * decPart == 0)
                {
                    return new StringEval("0");
                }
                String[] parts = s1.Split(' ');
                String[] fractParts;
                if (parts.Length == 2)
                {
                    fractParts = parts[1].Split('/');
                }
                else
                {
                    fractParts = s1.Split('/');
                }

                if (fractParts.Length == 2)
                {
                    double minVal = 1.0;
                    double currDenom = Math.Pow(10, fractParts[1].Length) - 1d;
                    double currNeum = 0;
                    for (int i = (int)(Math.Pow(10, fractParts[1].Length) - 1d); i > 0; i--)
                    {
                        for (int i2 = (int)(Math.Pow(10, fractParts[1].Length) - 1d); i2 > 0; i2--)
                        {
                            if (minVal >= Math.Abs((double)i2 / (double)i - decPart))
                            {
                                currDenom = i;
                                currNeum = i2;
                                minVal = Math.Abs((double)i2 / (double)i - decPart);
                            }
                        }
                    }
                    FormatBase neumFormatter = new DecimalFormat(fractParts[0]);
                    FormatBase denomFormatter = new DecimalFormat(fractParts[1]);
                    if (parts.Length == 2)
                    {
                        FormatBase wholeFormatter = new DecimalFormat(parts[0]);
                        String result = wholeFormatter.Format(wholePart) + " " + neumFormatter.Format(currNeum) + "/" + denomFormatter.Format(currDenom);
                        return new StringEval(result);
                    }
                    else
                    {
                        String result = neumFormatter.Format(currNeum + (currDenom * wholePart)) + "/" + denomFormatter.Format(currDenom);
                        return new StringEval(result);
                    }
                }
                else
                {
                    return ErrorEval.VALUE_INVALID;
                }
            }
            else
            {
                try
                {
                    FormatBase dateFormatter = new SimpleDateFormat(s1);
                    DateTime dt = new DateTime(1899, 11, 30, 0, 0, 0);
                    dt.AddDays((int)Math.Floor(s0));
                    double dayFraction = s0 - Math.Floor(s0);
                    dt.AddMilliseconds((int)Math.Round(dayFraction * 24 * 60 * 60 * 1000));
                    return new StringEval(dateFormatter.Format(dt));
                }
                catch (Exception)
                {
                    return ErrorEval.VALUE_INVALID;
                }
            }
        }
    }
}
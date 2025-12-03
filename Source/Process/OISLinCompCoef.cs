using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public class OISLinCompCoef
    {
        public const int PRE_ERROR = 5000;  // initial error vaule	
        public const int VT = 1;
        public const int OptSEG = 0;//OptParamSeg ON/OFF
        public const int OptALL = 1;//OptParamALL ON/OFF
        public const int RANGE = 4;


        public const int MAX_NUM_DATA = 65;// maximum number of data after Interpolation
        public const int MAX_NUM_DATA2 = 32;// maximum number of data after Averaging
        public const int MAX_NUM_DATA3 = 8;// maximum number of data about Extrem Candidate	
        public const int MIN_NUM_DATA = 6;// minimum number of data
        public const int NUM_COEF = 13;// size of output data array
        public const int NUM_COEF1 = 5;// size of output data array
        public const int NUM_COEF2_1 = 4;   // size of output data array
        public const int NUM_COEF2_2 = 7;   // size of output data array

        public const int BIT_COEF1 = 8;
        public const int BIT_COEF10 = 5;
        public const int BIT_COEF1G = 3;
        public const int BIT_COEF2 = 7;
        public const int BIT_COEF2G = 2;

        public const int COEF_X0 = 0;
        public const int COEF_X1 = 1;
        public const int COEF_X2 = 2;
        public const int COEF_X3 = 3;
        public const int COEF_GF = 4;
        public const int COEF_XA = 0;
        public const int COEF_XB = 1;
        public const int COEF_XC = 2;
        public const int COEF_XD = 3;
        public const int COEF_Y0 = 4;
        public const int COEF_YA = 5;
        public const int COEF_YB = 6;
        public const int COEF_YC = 7;
        public const int COEF_YD = 8;
        public const int COEF_YM = 9;
        public const int COEF_GP = 10;

        public const int ERROR_PREDATA = 1; // PreData() error
        public const int ERROR_CONVDATA = 2;    // ConvData() error
        public const int ERROR_CALINL0 = 3; // CalINL0() error
        public const int ERROR_CALCOEF1 = 4;    // CalLinCompCoef1() error
        public const int ERROR_CALINL1 = 5; // CalINL1() error
        public const int ERROR_CALCOEF2 = 6;    // CalLinCompCoef2() error
        public const int ERROR_CALINL2 = 7;  // CalINL2() error
        public const int ERROR_CONVREG = 8; // ConvReg() error

        float[] sarrX13 = new float[MAX_NUM_DATA];
        float[] snorX = new float[MAX_NUM_DATA];
        float[] sarrY13 = new float[MAX_NUM_DATA];
        float[] snorY = new float[MAX_NUM_DATA];
        float[] sarrIDEAL = new float[MAX_NUM_DATA];
        float[] sarrIdeal = new float[MAX_NUM_DATA];
        float[] sarrINL0 = new float[MAX_NUM_DATA];
        float[] sarrINL1 = new float[MAX_NUM_DATA];
        float[] sarrINL2 = new float[MAX_NUM_DATA];
        float[] sarrInl0 = new float[MAX_NUM_DATA];
        float[] sarrInl1 = new float[MAX_NUM_DATA];
        float[] sarrInl2 = new float[MAX_NUM_DATA];
        float[] sarrLin1 = new float[MAX_NUM_DATA];
        float[] sarrLin2 = new float[MAX_NUM_DATA];
        int[] sarrRegCoef1 = new int[NUM_COEF1];
        int[] sarrRegCoef2 = new int[NUM_COEF2_1 + NUM_COEF2_2];

        float[] sSlope = new float[MAX_NUM_DATA];
        float[] sIntercept = new float[MAX_NUM_DATA];

        float[] sarrCoef1 = new float[NUM_COEF1];
        float[] snorXLNC = new float[MAX_NUM_DATA];
        float[] snorYLNC = new float[MAX_NUM_DATA];
        float[] sarrX13LNC = new float[MAX_NUM_DATA];
        float[] sarrY13LNC = new float[MAX_NUM_DATA];

        float[] sarrNorCoef2 = new float[NUM_COEF2_1 + NUM_COEF2_2];

        float[] sposiTop = new float[MAX_NUM_DATA];
        float[] sposiBot = new float[MAX_NUM_DATA];
        float[] sposiPlus = new float[MAX_NUM_DATA];
        float[] sposiAD0 = new float[MAX_NUM_DATA];
        float[] sposiAD1 = new float[MAX_NUM_DATA];
        float[] sposiPrelin = new float[MAX_NUM_DATA];

        float[] sarrCoef2 = new float[NUM_COEF2_1 + NUM_COEF2_2];



        public int LinCompMain(float[] targPosi, float[] lensPosi, int numData, int pVt, int nVt, int ignInf, int ignMac, ref int[] linCoef, ref float resError)
        {
            // Initialize output data
            for (int i = 0; i < NUM_COEF; i++) { linCoef[i] = 0; }
            resError = PRE_ERROR;

            //Data preparation
            ringBufLinComp lineData = new ringBufLinComp();                            // Ring buffer for input data processing

            for (int i = 0; i < numData; i++)
            {
                lineData.dataX[i + 1] = targPosi[i];
                lineData.dataY[i + 1] = lensPosi[i];
            }

            lineData.startIndex = 1;
            lineData.endIndex = numData;
            lineData.size = numData;

            float spPos = 0.0f;
            float snPos = 0.0f;
            if (PreData(ref lineData, pVt, nVt, ignInf, ignMac, ref spPos, ref snPos) != 0) { return ERROR_PREDATA; }

            // Normalization of data
            float smaxX = 0;
            float sminX = 0;
            float smaxY = 0;
            float sminY = 0;
            smaxY = lineData.dataY[numData];
            sminY = lineData.dataY[1];

            //int MAX_NUM_DATA	= MAX_NUM_DATA;
            int posibit = 10;
            if (ConvData(lineData, posibit, spPos, snPos, sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, ref smaxX, ref sminX, ref smaxY, ref sminY) != 0) { return ERROR_CONVDATA; }

            // INL calculation before LNC at Normalized value
            if (CalINL0(sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, sarrINL0, sarrInl0) != 0) { return ERROR_CALINL0; }

            // Calculation of line compensation coefficients1
            for (int i = COEF_X0; i < COEF_GF + 1; i++) { sarrRegCoef1[i] = 0; }

            if (VT == 0) { if (CalLinCompCoef1(ref snorX, ref sarrInl0, sarrRegCoef1) != 0) { return ERROR_CALCOEF1; } }
            if (VT == 1) { if (CalLinCompCoef1(ref snorY, ref sarrInl0, sarrRegCoef1) != 0) { return ERROR_CALCOEF1; } }

            // INL calculation after LNC1
            if (CalINL1(sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, sarrINL1, sarrInl1, sarrLin1, sarrRegCoef1, spPos, snPos, smaxX, sminX, smaxY, sminY) != 0) { return ERROR_CALINL1; }

            // Calculation of line compensation coefficients2
            for (int i = COEF_XA; i < COEF_GP + 1; i++) { sarrRegCoef2[i] = 0; }
            sarrCoef2[COEF_Y0] = sarrINL1[0];
            sarrCoef2[COEF_YM] = sarrINL1[MAX_NUM_DATA - 1];
            CheckParam2(sarrCoef2, sarrRegCoef2, smaxY, sminY, spPos, snPos);

            if (VT == 0) { if (CalLinCompCoef2(snorX, sarrInl1, snorY, sarrX13, sarrY13, spPos, snPos, smaxX, sminX, smaxY, sminY, sarrIDEAL, sarrIdeal, sarrLin1, sarrRegCoef2, sarrINL2, sarrLin2, sarrInl2) != 0) { return ERROR_CALCOEF2; } }
            if (VT == 1) { if (CalLinCompCoef2(snorY, sarrInl1, snorX, sarrX13, sarrY13, spPos, snPos, smaxX, sminX, smaxY, sminY, sarrIDEAL, sarrIdeal, sarrLin1, sarrRegCoef2, sarrINL2, sarrLin2, sarrInl2) != 0) { return ERROR_CALCOEF2; } }

            // INL calculation after LNC2
            if (CalINL2(sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, sarrINL2, sarrLin2, sarrInl2, sarrRegCoef2, spPos, snPos, smaxX, sminX, smaxY, sminY, sarrLin1, 1) != 0) { return ERROR_CALINL2; }

            // Residual INL2 Error
            resError = 0;
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                if (AKM_Abs(sarrINL2[i]) > resError) { resError = AKM_Abs(sarrINL2[i]); }
            }

            // Register Address
            ConvReg(sarrRegCoef1, sarrRegCoef2, ref linCoef);

            /*
            //debug----------
            for(int i = 0; i < MAX_NUM_DATA; i++){printf("%f, %f, %f, %f, %f, %f, %f, %f\n", sarrX13[i], sarrINL0[i], sarrINL1[i], sarrINL2[i], snorX[i], sarrInl0[i], sarrInl1[i], sarrInl2[i]);}

            printf("\nCOEF1\n");
            for (int i = 0; i < NUM_COEF1; i++)					{printf("%d,", sarrRegCoef1[i]);	}
            printf("\nCOEF2\n");
            for (int i = 0; i < NUM_COEF2_1 + NUM_COEF2_2; i++)	{printf("%d,", sarrRegCoef2[i]);	}
            //-----------------
            */

            return 0;
        }
        int PreData(ref ringBufLinComp lineData, int pVt, int nVt, int ignInf, int ignMac, ref float pPos, ref float nPos)
        {
            //--------------------------------------------------
            //	Check of input data
            //--------------------------------------------------

            // Output error if number of valid data run short after delete the data within invalid range
            if (lineData.size < ignInf + ignMac + MIN_NUM_DATA) { return 1; }

            // Output error if target is not rising to the right
            for (int i = lineData.startIndex; i <= lineData.endIndex - 1; i++)
            {
              
                if (lineData.dataX[i] >= lineData.dataX[i + 1]) 
                { 
                    return 1; 
                }
            }

            //--------------------------------------------------
            //	Exclude the data within invalid range from processing range
            //--------------------------------------------------
            lineData.startIndex += ignInf;
            lineData.size -= (ignInf + ignMac);
            lineData.endIndex = lineData.startIndex + lineData.size - 1;

            //------------------------------------------------------
            //	Calculation of target before calibration based on correction value by VT setting
            //------------------------------------------------------

            // Correction value by POSVT
            if (pVt > 511) { pPos = ((pVt - 1024) / 1024.0f + 0.5f); }
            else { pPos = (pVt / 1024.0f + 0.5f); }

            if (nVt > 511) { nPos = ((nVt - 1024) / 1024.0f - 0.5f); }
            else { nPos = (nVt / 1024.0f - 0.5f); }

            return 0;
        }
        int ConvData(ringBufLinComp lineData, int posibit, float pPos, float nPos, float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, ref float maxX, ref float minX, ref float maxY, ref float minY)
        {
            // Resolution of Position
            if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 10)) { posibit = 10; }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 11)) { posibit = 11; }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 12)) { posibit = 12; }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 13)) { posibit = 13; }
            else { return 1; }

            //--------------------------------------------------------------------------------------
            //	Data interpolation
            //--------------------------------------------------------------------------------------

            float Lensmax = maxY;
            float Lensmin = minY;

            // Related Lens value
            for (int i = lineData.startIndex; i < lineData.endIndex + 1; i++)
            {
                lineData.dataY[i] -= Lensmin;
            }

            int k = 0;
            maxX = lineData.dataX[lineData.endIndex] * AKM_Pow(2.0f, 13 - posibit);
            minX = lineData.dataX[lineData.startIndex] * AKM_Pow(2.0f, 13 - posibit);
            maxY = lineData.dataY[lineData.startIndex];
            minY = lineData.dataY[lineData.startIndex];
            for (int i = lineData.startIndex; i < lineData.endIndex + 1; i++)
            {
                if (maxY < lineData.dataY[i]) { maxY = lineData.dataY[i]; }
                if (minY > lineData.dataY[i]) { minY = lineData.dataY[i]; }
            }

            if (VT == 0)
            {
                for (int i = 0; i < lineData.size - 1; i++)
                {
                    sSlope[i] = (lineData.dataY[lineData.startIndex + i + 1] - lineData.dataY[lineData.startIndex + i]) / (lineData.dataX[lineData.startIndex + i + 1] - lineData.dataX[lineData.startIndex + i]) / AKM_Pow(2.0f, 13 - posibit);
                    sIntercept[i] = lineData.dataY[lineData.startIndex + i] - sSlope[i] * lineData.dataX[lineData.startIndex + i] * AKM_Pow(2.0f, 13 - posibit);
                }
                sSlope[lineData.size - 1] = sSlope[lineData.size - 2];
                sIntercept[lineData.size - 1] = sIntercept[lineData.size - 2];

                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrX13[i] = (maxX - minX) / (MAX_NUM_DATA - 1) * i + minX;
                    if (arrX13[i] != 0 && arrX13[i] > lineData.dataX[lineData.startIndex + k + 1] * AKM_Pow(2.0f, 13 - posibit))
                    {
                        if (k < lineData.size - 1)
                        {
                            k = k + 1;
                        }
                    }
                    arrY13[i] = sSlope[k] * arrX13[i] + sIntercept[k];
                }
            }

            if (VT == 1)
            {
                for (int i = 0; i < lineData.size - 1; i++)
                {
                    sSlope[i] = (lineData.dataX[lineData.startIndex + i + 1] - lineData.dataX[lineData.startIndex + i]) / (lineData.dataY[lineData.startIndex + i + 1] - lineData.dataY[lineData.startIndex + i]) * AKM_Pow(2.0f, 13 - posibit);
                    sIntercept[i] = lineData.dataX[lineData.startIndex + i] * AKM_Pow(2.0f, 13 - posibit) - sSlope[i] * lineData.dataY[lineData.startIndex + i];
                }
                sSlope[lineData.size - 1] = sSlope[lineData.size - 2];
                sIntercept[lineData.size - 1] = sIntercept[lineData.size - 2];

                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrY13[i] = (maxY - minY) / (MAX_NUM_DATA - 1) * i + minY;
                    if (arrY13[i] != 0 && arrY13[i] > lineData.dataY[lineData.startIndex + k + 1])
                    {
                        if (k < lineData.size - 1)
                        {
                            k = k + 1;
                        }
                    }
                    arrX13[i] = sSlope[k] * arrY13[i] + sIntercept[k];
                }
            }


            //--------------------------------------------------------------------------------------
            //	Normalization
            //--------------------------------------------------------------------------------------

            maxY = Lensmax;
            minY = Lensmin;

            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                norX[i] = nPos + arrX13[i] * (pPos - nPos) / AKM_Pow(2.0f, 13);
                norY[i] = nPos + arrY13[i] * (pPos - nPos) / (maxY - minY);
            }


            //--------------------------------------------------------------------------------------
            //	Ideal Data
            //--------------------------------------------------------------------------------------

            float SlopeIDEAL, InterceptIDEAL;
            float SlopeIdeal, InterceptIdeal;

            if (VT == 0)
            {
                SlopeIDEAL = (lineData.dataY[lineData.endIndex] - lineData.dataY[lineData.startIndex]) / (maxX - minX);
                InterceptIDEAL = lineData.dataY[lineData.startIndex] - SlopeIDEAL * minX;

                SlopeIdeal = (norY[MAX_NUM_DATA - 1] - norY[0]) / (norX[MAX_NUM_DATA - 1] - norX[0]);
                InterceptIdeal = norY[0] - SlopeIdeal * norX[0];

                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrIDEAL[i] = SlopeIDEAL * arrX13[i] + InterceptIDEAL;
                    arrIdeal[i] = SlopeIdeal * norX[i] + InterceptIdeal;
                }
            }

            if (VT == 1)
            {
                SlopeIDEAL = (lineData.dataY[lineData.endIndex] - lineData.dataY[lineData.startIndex]) / (maxX - minX);
                InterceptIDEAL = lineData.dataY[lineData.startIndex] - SlopeIDEAL * minX;

                SlopeIdeal = (norX[MAX_NUM_DATA - 1] - norX[0]) / (norY[MAX_NUM_DATA - 1] - norY[0]);
                InterceptIdeal = norX[0] - SlopeIdeal * norY[0];

                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrIDEAL[i] = SlopeIDEAL * arrX13[i] + InterceptIDEAL;
                    arrIdeal[i] = SlopeIdeal * norY[i] + InterceptIdeal;
                }
            }

            return 0;
        }
        float AKM_Pow(float dbData, int num)
        {
            float pow = 1.0f;

            for (int i = 0; i < num; i++)
            {
                pow *= dbData;
            }

            return pow;
        }
        int CalINL0(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL0, float[] arrInl0)
        {
            // -------------------------------------
            // Normalized Data 
            // -------------------------------------

            // Calculation of the difference value between the ideal value and the mesuared value when the target position to achieve a certain lens position
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                if (VT == 0) { arrInl0[i] = arrIdeal[i] - norY[i]; }
                if (VT == 1) { arrInl0[i] = norX[i] - arrIdeal[i]; }
            }

            // -------------------------------------
            // Target, Lens Position Data 
            // -------------------------------------

            // Calculation of the difference value between the ideal value and the mesuared value when the target position to achieve a certain lens position
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                if (VT == 0) { arrINL0[i] = arrIDEAL[i] - arrY13[i]; }
                if (VT == 1) { arrINL0[i] = arrIDEAL[i] - arrY13[i]; }
            }

            return 0;
        }
        int CalLinCompCoef1(ref float[] inputX, ref float[] arrINL0, int[] arrRegCoef1)
        {
            // Calculation of approximation coefficients 
            float[] arrCoef1 = new float[NUM_COEF1];
            Polynomial(ref inputX, ref arrINL0, 4, ref arrCoef1);

            // Adjust to Register Range 
            CheckParam1(arrCoef1, arrRegCoef1);

            return 0;
        }
        void Polynomial(ref float[] dbDataX, ref float[] dbDataY, int numCoef, ref float[] dbCoef)
        {
            float[,] arrayLSM = new float[4, 5];   // Array of calculating coefficient.(LSM:Least Squares Method)

            // Initialize array.
            for (int i = 0; i < numCoef; i++)
            {
                for (int j = 0; j < numCoef + 1; j++)
                {
                    arrayLSM[i, j] = 0;
                }
            }

            // Calculation element and assignment **************************************************
            for (int i = 0; i < numCoef; i++)
            {
                for (int j = 0; j < numCoef; j++)
                {
                    for (int k = 0; k < MAX_NUM_DATA; k++)
                    {
                        arrayLSM[i, j] += AKM_Pow(dbDataX[k], i + j);
                    }
                }
            }

            for (int i = 0; i < numCoef; i++)
            {
                for (int k = 0; k < MAX_NUM_DATA; k++)
                {
                    arrayLSM[i, numCoef] += (AKM_Pow(dbDataX[k], i) * dbDataY[k]);
                }
            }
            // *******************************************************************

            // Calculation by gaussian elimination.
            Gauss(arrayLSM, numCoef, ref dbCoef);
        }
        void Gauss(float[,] arrayLSM, int numCoef, ref float[] dbCoef)
        {
            // Sort
            for (int i = 0; i < numCoef; i++)
            {
                float dbMax = 0;
                int iPivot = i;

                // Search for the largest row.
                for (int l = i; l < numCoef; l++)
                {
                    if (dbMax < AKM_Abs(arrayLSM[l, i]))
                    {
                        dbMax = AKM_Abs(arrayLSM[l, i]);
                        iPivot = l;
                    }
                }

                // Replacing rows
                if (iPivot != i)
                {
                    float dbTemp = 0;
                    for (int j = 0; j < numCoef + 1; j++)
                    {
                        dbTemp = arrayLSM[i, j];
                        arrayLSM[i, j] = arrayLSM[iPivot, j];
                        arrayLSM[iPivot, j] = dbTemp;
                    }
                }
            }

            // Forward elimination
            for (int k = 0; k < numCoef; k++)
            {
                float dbTemp1 = arrayLSM[k, k];
                arrayLSM[k, k] = 1;

                for (int j = k + 1; j < numCoef + 1; j++)
                {
                    arrayLSM[k, j] /= dbTemp1;
                }

                for (int i = k + 1; i < numCoef; i++)
                {
                    float dbTemp2 = arrayLSM[i, k];

                    for (int j = k + 1; j < numCoef + 1; j++)
                    {
                        arrayLSM[i, j] -= dbTemp2 * arrayLSM[k, j];
                    }

                    arrayLSM[i, k] = 0;
                }
            }

            // backward substitution
            for (int i = numCoef - 1; i >= 0; i--)
            {
                dbCoef[i] = arrayLSM[i, numCoef];

                for (int j = numCoef - 1; i < j; j--)
                {
                    dbCoef[i] -= arrayLSM[i, j] * dbCoef[j];
                }
            }
        }
        float AKM_Abs(float dbData)
        {
            float abs = dbData;

            if (dbData < 0) { abs = dbData * -1.0f; }

            return abs;
        }
        int CheckParam1(float[] arrCoef1, int[] arrRegCoef1)
        {
            float[] arrCoef1Can = new float[NUM_COEF1];

            // Adjustment approximation coefficients including LinGainF
            for (int i = 0; i < AKM_Pow(2.0f, BIT_COEF1G); i++)
            {
                arrCoef1Can[COEF_GF] = 0.5f * (i + 2);

                for (int j = 0; j < NUM_COEF1 - 1; j++)
                {
                    arrCoef1Can[j] = (int)(arrCoef1[j] / arrCoef1Can[COEF_GF] * AKM_Pow(2.0f, BIT_COEF1 - 1)) / AKM_Pow(2.0f, BIT_COEF1 - 1);
                }

                if (arrCoef1Can[COEF_X0] > -0.125f && arrCoef1Can[COEF_X0] < 0.125f && arrCoef1Can[COEF_X1] > -1.0f && arrCoef1Can[COEF_X1] < 1.0f
                  && arrCoef1Can[COEF_X2] > -1.0f && arrCoef1Can[COEF_X2] < 1.0f && arrCoef1Can[COEF_X3] > -1.0f && arrCoef1Can[COEF_X3] < 1.0f)
                {
                    break;
                }

            }

            // Calculation of Register value
            for (int i = 1; i < NUM_COEF1 - 1; i++)
            {
                arrRegCoef1[i] = (int)(arrCoef1Can[i] * AKM_Pow(2.0f, BIT_COEF1 - 1));
                if (arrRegCoef1[i] < 0) { arrRegCoef1[i] += (int)AKM_Pow(2.0f, BIT_COEF1); }
            }
            arrRegCoef1[COEF_X0] = (int)(arrCoef1Can[COEF_X0] * AKM_Pow(2.0f, BIT_COEF1 - 1));
            if (arrRegCoef1[COEF_X0] < 0) { arrRegCoef1[COEF_X0] += (int)AKM_Pow(2.0f, BIT_COEF10); }

            arrRegCoef1[COEF_GF] = (int)((arrCoef1Can[COEF_GF] - 1.0f) * 2.0f);

            return 0;
        }
        int CalINL1(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL1, float[] arrInl1, float[] arrLin1, int[] arrRegCoef1, float pPos, float nPos, float maxX, float minX, float maxY, float minY)
        {
            // Calclation of the compensation amount1 
            if (arrRegCoef1[COEF_X0] > AKM_Pow(2.0f, BIT_COEF10 - 1)) { sarrCoef1[COEF_X0] = (arrRegCoef1[COEF_X0] - AKM_Pow(2.0f, BIT_COEF10)) / AKM_Pow(2.0f, BIT_COEF1 - 1); }
            else { sarrCoef1[COEF_X0] = arrRegCoef1[COEF_X0] / AKM_Pow(2.0f, BIT_COEF1 - 1); }

            for (int i = 1; i < NUM_COEF1 - 1; i++)
            {
                if (arrRegCoef1[i] > AKM_Pow(2.0f, BIT_COEF1 - 1)) { sarrCoef1[i] = (arrRegCoef1[i] - AKM_Pow(2.0f, BIT_COEF1)) / AKM_Pow(2.0f, BIT_COEF1 - 1); }
                else { sarrCoef1[i] = arrRegCoef1[i] / AKM_Pow(2.0f, BIT_COEF1 - 1); }
            }
            sarrCoef1[COEF_GF] = arrRegCoef1[COEF_GF] * 0.5f + 1.0f;


            if (VT == 0) { CalLinGain1(norX, sarrCoef1, pPos, nPos, arrLin1); }
            if (VT == 1) { CalLinGain1(norY, sarrCoef1, pPos, nPos, arrLin1); }

            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                snorXLNC[i] = norX[i] - arrLin1[i];
                sarrX13LNC[i] = (snorXLNC[i] - nPos) * AKM_Pow(2.0f, 13) / (pPos - nPos);
            }

            // -------------------------------------
            // Normalized Data 
            // -------------------------------------

            // Calculation of the INL after LNC1 at Normalized Target data
            if (VT == 0)
            {
                CalApprox(snorXLNC, norY, norX, snorYLNC, MAX_NUM_DATA, MAX_NUM_DATA);
                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl1[i] = arrIdeal[i] - snorYLNC[i];
                }
            }

            if (VT == 1)
            {
                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl1[i] = (snorXLNC[i]) - arrIdeal[i];
                }
            }

            // -------------------------------------
            // Target, Lens Position Data 
            // -------------------------------------

            // Calculation of the INL after LNC1 at Lens position data

            if (VT == 0)
            {
                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrINL1[i] = arrIDEAL[i] - (arrY13[i] + arrLin1[i] / (pPos - nPos) * (maxY - minY));
                }
            }

            if (VT == 1)
            {
                CalApprox(sarrX13LNC, arrY13, arrX13, sarrY13LNC, MAX_NUM_DATA, MAX_NUM_DATA);

                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrINL1[i] = arrIDEAL[i] - sarrY13LNC[i];
                }
            }

            return 0;
        }

        int CalLinGain1(float[] inputX, float[] arrCoef1, float pPos, float nPos, float[] arrLin1)
        {
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                sposiTop[i] = (int)(inputX[i] * AKM_Pow(2.0f, 13) / AKM_Pow(2.0f, 7)) * AKM_Pow(2.0f, 7) / AKM_Pow(2.0f, 13);
                sposiBot[i] = AKM_Abs(sposiTop[i] - inputX[i]) * AKM_Pow(2.0f, 13) / AKM_Pow(2.0f, 7);
                if (inputX[i] > 0) { sposiPlus[i] = sposiTop[i] * AKM_Pow(2.0f, 6) + 1; } else { sposiPlus[i] = sposiTop[i] * AKM_Pow(2.0f, 6) - 1; }
                if (sposiPlus[i] / AKM_Pow(2.0f, 6) < -1) { sposiPlus[i] = -1; } else { sposiPlus[i] = sposiPlus[i] / AKM_Pow(2.0f, 6); }

                sposiAD0[i] = AKM_Round((arrCoef1[COEF_X3] * AKM_Pow(sposiTop[i], 3) + arrCoef1[COEF_X2] * AKM_Pow(sposiTop[i], 2) + arrCoef1[COEF_X1] * sposiTop[i] + arrCoef1[COEF_X0]) * AKM_Pow(2.0f, 11)) / AKM_Pow(2.0f, 14);
                sposiAD1[i] = AKM_Round((arrCoef1[COEF_X3] * AKM_Pow(sposiPlus[i], 3) + arrCoef1[COEF_X2] * AKM_Pow(sposiPlus[i], 2) + arrCoef1[COEF_X1] * sposiPlus[i] + arrCoef1[COEF_X0]) * AKM_Pow(2.0f, 11)) / AKM_Pow(2.0f, 14);
                sposiPrelin[i] = (int)((sposiAD1[i] - sposiAD0[i]) * AKM_Pow(2.0f, 20)) / AKM_Pow(2.0f, 20) * sposiBot[i] + sposiAD0[i];
                arrLin1[i] = AKM_Round(((int)(sposiPrelin[i] * AKM_Pow(2.0f, 17)) / AKM_Pow(2.0f, 17)) * AKM_Pow(2.0f, 16)) / AKM_Pow(2.0f, 13) * arrCoef1[COEF_GF];

            }

            return 0;
        }
        int AKM_Round(float dbData)
        {
            float round = 0;

            if (dbData > 0) { round = dbData + 0.5f; }
            else if (dbData < 0) { round = dbData - 0.5f; }

            return (int)round;
        }
        int CalApprox(float[] inputX, float[] inputY, float[] outputX, float[] outputY, int numData1, int numData2)
        {
            for (int i = 0; i < numData2; i++)
            {
                if (outputX[i] < inputX[0])
                {
                    int k = 0;
                    float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                    float offset = inputY[k] - slope * inputX[k];

                    outputY[i] = slope * outputX[i] + offset;
                }
                else
                {
                    for (int k = 0; k < numData1 - 1; k++)
                    {
                        if (inputX[k + 1] >= outputX[i] && inputX[k] <= outputX[i])
                        {

                            // Output error if there is dead zone
                            if (inputX[k + 1] == inputX[k]) { return 1; }

                            float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                            float offset = inputY[k] - slope * inputX[k];

                            outputY[i] = slope * outputX[i] + offset;
                            break;
                        }
                        else if (outputX[i] > inputX[numData1 - 1])
                        {
                            k = numData1 - 2;
                            float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                            float offset = inputY[k] - slope * inputX[k];

                            outputY[i] = slope * outputX[i] + offset;
                        }
                    }
                }
            }

            return 0;
        }
        int CheckParam2(float[] arrCoef2, int[] arrRegCoef2, float maxY, float minY, float pPos, float nPos)
        {
            // Adjustment approximation coefficients including LinGainP
            for (int i = 0; i < AKM_Pow(2.0f, BIT_COEF2G); i++)
            {
                arrCoef2[COEF_GP] = (float)i + 1.0f;

                for (int j = 0; j < NUM_COEF2_2 - 1; j++)
                {
                    arrRegCoef2[NUM_COEF2_1 + j] = (int)(arrCoef2[NUM_COEF2_1 + j] / (maxY - minY) * AKM_Pow(2.0f, 4) * AKM_Pow(2.0f, 6) * (pPos - nPos) / arrCoef2[COEF_GP]);
                }

                if (arrRegCoef2[4] >= -64 && arrRegCoef2[4] <= 63 && arrRegCoef2[5] >= -64 && arrRegCoef2[5] <= 63
                  && arrRegCoef2[6] >= -64 && arrRegCoef2[6] <= 63 && arrRegCoef2[7] >= -64 && arrRegCoef2[7] <= 63
                  && arrRegCoef2[8] >= -64 && arrRegCoef2[8] <= 63 && arrRegCoef2[9] >= -64 && arrRegCoef2[9] <= 63)
                {
                    break;
                }
            }

            // Calculation of Register value
            for (int i = 0; i < NUM_COEF2_1; i++)
            {
                arrRegCoef2[i] = AKM_Round(arrCoef2[i] / AKM_Pow(2.0f, 13) * AKM_Pow(2.0f, 7));
            }

            for (int i = 0; i < NUM_COEF2_2 - 1; i++)
            {
                if (arrRegCoef2[NUM_COEF2_1 + i] < 0) { arrRegCoef2[NUM_COEF2_1 + i] += 128; }
            }

            arrRegCoef2[COEF_GP] = (int)arrCoef2[COEF_GP] - 1;


            return 0;
        }
        int CalLinCompCoef2(float[] norX, float[] arrINL1, float[] norY, float[] arrX13, float[] arrY13, float pPos, float nPos, float maxX, float minX, float maxY, float minY, float[] arrIDEAL, float[] arrIdeal, float[] arrLin1, int[] arrRegCoef2, float[] arrINL2, float[] arrLin2, float[] arrInl2)
        {

            // Averaging
            int numAve = (MAX_NUM_DATA - 1) / MAX_NUM_DATA2;
            int numDataAve = MAX_NUM_DATA2;
            float[] arrXthin = new float[MAX_NUM_DATA2];
            float[] arrINL1sum = new float[MAX_NUM_DATA2];
            float[] arrINL1ave = new float[MAX_NUM_DATA2];
            float[] SlopeINLAve = new float[MAX_NUM_DATA2];
            float[] SlopeVal = new float[MAX_NUM_DATA2];
            int[] SlopeValRank = new int[MAX_NUM_DATA2];

            // Candidate
            float[] arrExtremXCan = new float[MAX_NUM_DATA2];
            float[] arrExtremX = new float[MAX_NUM_DATA3];
            float[] arrExtremY = new float[MAX_NUM_DATA2];
            int[] arrJudgeExtrem = new int[MAX_NUM_DATA2];

            int[] ExtremXRank = new int[MAX_NUM_DATA3];
            float[] arrCoef2CanX = new float[MAX_NUM_DATA3];
            float[] arrNorCoef2CanX = new float[MAX_NUM_DATA3];
            int[] arrRegCoef2CanX = new int[MAX_NUM_DATA3];
            float[] arrCoef2CanY = new float[MAX_NUM_DATA3];
            float[] arrNorCoef2CanY = new float[MAX_NUM_DATA3];
            int[] arrRegCoef2CanY = new int[MAX_NUM_DATA3];


            //Initialization
            int num = 0;
            int k = 0;
            for (int i = 0; i < MAX_NUM_DATA2; i++)
            {
                arrINL1sum[i] = 0.0f;
                arrExtremXCan[i] = 10;
                arrExtremY[i] = 0;
                arrJudgeExtrem[i] = 0;
                SlopeValRank[i] = 10;
            }

            for (int i = 0; i < MAX_NUM_DATA3; i++)
            {
                arrExtremX[i] = 10;
                arrCoef2CanX[i] = -1;
                ExtremXRank[i] = 10;
                arrCoef2CanY[i] = 0;
                arrNorCoef2CanX[i] = 0;
                arrNorCoef2CanY[i] = 0;
                arrRegCoef2CanX[i] = 0;
                arrRegCoef2CanY[i] = 0;
            }

            for (int i = COEF_XA; i < COEF_GP + 1; i++) { sarrCoef2[i] = 0; }

            // Averaging Using normalized data
            float maxnorX = norX[0];
            float minnorX = norX[0];
            float maxnorY = norY[0];
            float minnorY = norY[0];

            for (int i = 1; i < MAX_NUM_DATA; i++)
            {
                if (maxnorX < norX[i]) { maxnorX = norX[i]; }
                if (minnorX > norX[i]) { minnorX = norX[i]; }
                if (maxnorY < norY[i]) { maxnorY = norY[i]; }
                if (minnorY > norY[i]) { minnorY = norY[i]; }
            }

            for (int i = 0; i < MAX_NUM_DATA - 1; i++)
            {
                if (i == numAve * (k + 1))
                {
                    arrXthin[k] = (i - num / 2.0f) * ((maxnorX - minnorX) / (MAX_NUM_DATA - 1)) + minnorX;
                    k += 1;
                    if (k == MAX_NUM_DATA2) { break; }
                    num = 0;
                }
                arrINL1sum[k] += arrINL1[i];
                num += 1;
                arrINL1ave[k] = arrINL1sum[k] / num;
            }
            arrXthin[numDataAve - 1] = (MAX_NUM_DATA - 1 - num / 2.0f) * ((maxnorX - minnorX) / (MAX_NUM_DATA - 1)) + minnorX;

            if (k < (MAX_NUM_DATA - 1) / numAve)
            {
                arrXthin[k] = (numAve * k + num / 2.0f) * ((maxnorX - minnorX) / (MAX_NUM_DATA - 1)) + minnorX;
            }

            // Calculation of Slope
            for (int i = 0; i < numDataAve - 1; i++)
            {
                SlopeINLAve[i] = (arrINL1ave[i + 1] - arrINL1ave[i]) / (arrXthin[i + 1] - arrXthin[i]);
            }

            // Calculation of Slope Valiation
            for (int i = 0; i < numDataAve - 2; i++)
            {
                SlopeVal[i] = AKM_Abs(SlopeINLAve[i + 1] - SlopeINLAve[i]);
            }

            // X Candidate Generation from Extrem values
            int numExtrem = 0;
            int r = 0;

            for (int i = 1; i < numDataAve - 1; i++)
            {
                if (SlopeINLAve[i] * SlopeINLAve[i - 1] > 0) { arrJudgeExtrem[i] = 0; }
                else { arrJudgeExtrem[i] = 1; arrExtremXCan[r] = arrXthin[i]; arrExtremY[r] = arrINL1ave[i]; r += 1; }
                numExtrem += arrJudgeExtrem[i];
            }

            if (numExtrem > MAX_NUM_DATA3 / 2)
            {
                float[] arrExtremYVal = new float[MAX_NUM_DATA2];
                int[] ExtremYRank = new int[MAX_NUM_DATA2];

                for (int i = 0; i < numExtrem - 1; i++)
                {
                    arrExtremYVal[i] = AKM_Abs(arrExtremY[i + 1] - arrExtremY[i]);
                }

                Rank(numExtrem - 1, arrExtremYVal, 1, ExtremYRank);

                for (int i = 0; i < numExtrem - 1; i++)
                {
                    if (ExtremYRank[i] < MAX_NUM_DATA3 / 2)
                    {
                        arrExtremX[ExtremYRank[i]] = arrExtremXCan[i];
                    }
                }
                numExtrem = MAX_NUM_DATA3 / 2;
            }
            else
            {
                for (int i = 0; i < numExtrem; i++)
                {
                    arrExtremX[i] = arrExtremXCan[i];
                }
            }


            // X Candidate Generation from Slope Valuation Ranking
            // Slope Vaulation Ranking
            Rank(numDataAve - 2, SlopeVal, 1, SlopeValRank);

            // Select Extreme X Value	
            int s = -10;
            r = 0;

            for (int rank = 0; rank < numDataAve - 2; rank++)
            {
                for (int i = 0; i < numDataAve - 2; i++)
                {
                    if (SlopeValRank[i] == rank)
                    {
                        if (i != s - 1 && i != s + 1)
                        {                       // Excluding front and back point about Etreme X value of upper ranking
                            arrExtremX[numExtrem + r] = arrXthin[i + 1];
                            s = i;
                            r += 1;
                        }
                    }
                }
                if (r == MAX_NUM_DATA3 - numExtrem) { break; }
            }

            // Remove identical Candidate
            int numSame = 0;
            for (int i = 0; i < numExtrem; i++)
            {
                for (int j = 0; j < MAX_NUM_DATA3 - numExtrem; j++)
                    if (arrExtremX[i] == arrExtremX[numExtrem + j])
                    {
                        arrExtremX[numExtrem + j] = 10;
                        numSame += 1;
                    }
            }

            // Extreme X Value Ranking
            int numCandidate = MAX_NUM_DATA3 - numSame;
            Rank(MAX_NUM_DATA3, arrExtremX, 0, ExtremXRank);

            // Sort Extreme X Value
            for (int i = 0; i < MAX_NUM_DATA3; i++)
            {
                arrCoef2CanX[ExtremXRank[i]] = (arrExtremX[i] - minnorX) * AKM_Pow(2.0f, 13) / (maxnorX - minnorX);
            }

            for (int i = 0; i < numCandidate; i++)
            {
                arrNorCoef2CanX[i] = minnorX + arrCoef2CanX[i] / AKM_Pow(2.0f, 13) * (maxnorX - minnorX);
            }

            CalApprox(norX, arrINL1, arrNorCoef2CanX, arrNorCoef2CanY, MAX_NUM_DATA, numCandidate);

            // Calculation of Register value
            for (int i = 0; i < numCandidate; i++)
            {
                arrRegCoef2CanX[i] = AKM_Round(arrCoef2CanX[i] / AKM_Pow(2.0f, 13) * AKM_Pow(2.0f, 7));

                arrCoef2CanY[i] = arrNorCoef2CanY[i] * (maxY - minY) / (maxnorY - minnorY);
            }

            for (int i = 0; i < numCandidate; i++)
            {
                arrRegCoef2CanY[i] = (int)(arrCoef2CanY[i] / (maxY - minY) * AKM_Pow(2.0f, 4) * AKM_Pow(2.0f, 6) * (pPos - nPos) / (sarrRegCoef2[COEF_GP] + 1));
                if (arrRegCoef2CanY[i] < 0) { arrRegCoef2CanY[i] += 128; }
            }

            //-----------------------------------
            // Setting xa,xb,xc,xd
            //-----------------------------------
            if (VT == 0) { OptParamX(arrX13, arrY13, norX, norY, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2CanX, arrRegCoef2CanY, numCandidate, arrRegCoef2, arrINL2, arrLin2, arrInl2); }
            if (VT == 1) { OptParamX(arrX13, arrY13, norY, norX, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2CanX, arrRegCoef2CanY, numCandidate, arrRegCoef2, arrINL2, arrLin2, arrInl2); }

            // Check residual error after OptParamX
            float ResError_Tempo = 0.0f;
            float ResIntError_Tempo = 0.0f;
            int[] arrRegCoef2_Tempo = new int[NUM_COEF2_2];

            if (VT == 0) { CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }
            if (VT == 1) { CalINL2(arrX13, arrY13, norY, norX, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }

            for (int i = 0; i < NUM_COEF2_2; i++) { arrRegCoef2_Tempo[i] = arrRegCoef2[NUM_COEF2_1 + i]; }

            for (int m = 0; m < MAX_NUM_DATA; m++)
            {
                if (AKM_Abs(arrINL2[m]) > ResError_Tempo) { ResError_Tempo = AKM_Abs(arrINL2[m]); }
                ResIntError_Tempo += AKM_Abs(arrINL2[m]);
            }


            //-----------------------------------
            // Setting y0, ya, yb, yc, yd, ym, GainP 
            //-----------------------------------

            // Optimisation of arrCoef2 about NUM_COEF_2_2 
            // Optimized for Segment0 겏 4 겏 1?3
            if (OptSEG == 1)
            {
                if (VT == 0)
                {
                    for (int times = 0; times < 3; times++)
                    {
                        OptParamSeg(arrX13, arrY13, norX, norY, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2, arrINL2, arrLin2, arrInl2, times);
                    }
                }
                if (VT == 1)
                {
                    for (int times = 0; times < 3; times++)
                    {
                        OptParamSeg(arrX13, arrY13, norY, norX, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2, arrINL2, arrLin2, arrInl2, times);
                    }
                }

                // Check residual error after OptParamSeg
                float ResError_OptSeg = 0.0f;
                float ResIntError_OptSeg = 0.0f;
                int[] arrRegCoef2_OptSeg = new int[NUM_COEF2_2];

                if (VT == 0) { CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }
                if (VT == 1) { CalINL2(arrX13, arrY13, norY, norX, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }

                for (int i = 0; i < NUM_COEF2_2; i++) { arrRegCoef2_OptSeg[i] = arrRegCoef2[NUM_COEF2_1 + i]; }

                for (int m = 0; m < MAX_NUM_DATA; m++)
                {
                    if (AKM_Abs(arrINL2[m]) > ResError_OptSeg) { ResError_OptSeg = AKM_Abs(arrINL2[m]); }
                    ResIntError_OptSeg += AKM_Abs(arrINL2[m]);
                }

                if (ResError_OptSeg > ResError_Tempo || (ResError_OptSeg == ResError_Tempo && ResIntError_OptSeg > ResIntError_Tempo))
                {
                    for (int i = 0; i < NUM_COEF2_2; i++) { arrRegCoef2[NUM_COEF2_1 + i] = arrRegCoef2_Tempo[i]; }
                }
                else
                {
                    for (int i = 0; i < NUM_COEF2_2; i++) { arrRegCoef2_Tempo[i] = arrRegCoef2_OptSeg[i]; }
                    ResError_Tempo = ResError_OptSeg;
                    ResIntError_Tempo = ResIntError_OptSeg;
                }

            }

            // Optimized for All Segment
            if (OptALL == 1)
            {
                if (VT == 0) { OptParamAll(arrX13, arrY13, norX, norY, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2, arrINL2, arrLin2, arrInl2); }
                if (VT == 1) { OptParamAll(arrX13, arrY13, norY, norX, pPos, nPos, maxX, minX, arrIDEAL, arrIdeal, arrLin1, arrRegCoef2, arrINL2, arrLin2, arrInl2); }

                // Check residual error after OptParamAll
                float ResError_OptAll = 0.0f;
                float ResIntError_OptAll = 0.0f;

                if (VT == 0) { CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }
                if (VT == 1) { CalINL2(arrX13, arrY13, norY, norX, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, maxY, minY, arrLin1, 1); }

                for (int m = 0; m < MAX_NUM_DATA; m++)
                {
                    if (AKM_Abs(arrINL2[m]) > ResError_OptAll) { ResError_OptAll = AKM_Abs(arrINL2[m]); }
                    ResIntError_OptAll += AKM_Abs(arrINL2[m]);
                }

                if (ResError_OptAll > ResError_Tempo || (ResError_OptAll == ResError_Tempo && ResIntError_OptAll > ResIntError_Tempo))
                {
                    for (int i = 0; i < NUM_COEF2_2; i++) { arrRegCoef2[NUM_COEF2_1 + i] = arrRegCoef2_Tempo[i]; }
                }

            }

            return 0;
        }
        void Rank(int num, float[] dbData, int direction, int[] dbDataRank)
        {
            for (int i = 0; i < num; i++)
            {
                dbDataRank[i] = 0;
            }

            if (direction == 0)
            {
                for (int i = 1; i < num; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (dbData[j] > dbData[i]) { dbDataRank[j]++; }
                        if (dbData[j] < dbData[i]) { dbDataRank[i]++; }
                    }
                }
            }
            if (direction == 1)
            {
                for (int i = 1; i < num; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (dbData[j] < dbData[i]) { dbDataRank[j]++; }
                        if (dbData[j] > dbData[i]) { dbDataRank[i]++; }
                    }
                }
            }

        }
        int OptParamX(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float pPos, float nPos, float maxX, float minX, float[] arrIDEAL, float[] arrIdeal, float[] arrLin1, int[] arrRegCoef2CanX, int[] arrRegCoef2CanY, int numCandidate, int[] arrRegCoef2, float[] arrINL2, float[] arrLin2, float[] arrInl2)
        {
            float resError = PRE_ERROR;
            float resIntError = PRE_ERROR;
            float ResError = 0.0f;
            float ResIntError = 0.0f;
            int a = 0, b = 0, c = 0, d = 0;

            for (int i = 0; i < numCandidate - 3; i++)
            {
                arrRegCoef2[COEF_XA] = arrRegCoef2CanX[i];
                arrRegCoef2[COEF_YA] = arrRegCoef2CanY[i];

                for (int j = i + 1; j < numCandidate - 2; j++)
                {
                    arrRegCoef2[COEF_XB] = arrRegCoef2CanX[j];
                    arrRegCoef2[COEF_YB] = arrRegCoef2CanY[j];

                    for (int k = j + 1; k < numCandidate - 1; k++)
                    {
                        arrRegCoef2[COEF_XC] = arrRegCoef2CanX[k];
                        arrRegCoef2[COEF_YC] = arrRegCoef2CanY[k];

                        for (int l = k + 1; l < numCandidate; l++)
                        {
                            arrRegCoef2[COEF_XD] = arrRegCoef2CanX[l];
                            arrRegCoef2[COEF_YD] = arrRegCoef2CanY[l];

                            CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, 500, 0, arrLin1, 0);

                            ResError = 0.0f;
                            ResIntError = 0.0f;

                            for (int m = 0; m < MAX_NUM_DATA; m++)
                            {
                                if (AKM_Abs(arrInl2[m]) > ResError) { ResError = AKM_Abs(arrInl2[m]); }
                                ResIntError += AKM_Abs(arrInl2[m]);
                            }

                            if (ResError < resError)
                            {
                                resError = ResError;
                                resIntError = ResIntError;
                                a = i; b = j; c = k; d = l;
                            }

                            if (ResError == resError)
                            {
                                if (ResIntError < resIntError)
                                {
                                    resIntError = ResIntError;
                                    a = i; b = j; c = k; d = l;
                                }
                            }

                        }
                    }
                }
            }

            arrRegCoef2[COEF_XA] = arrRegCoef2CanX[a];
            arrRegCoef2[COEF_XB] = arrRegCoef2CanX[b];
            arrRegCoef2[COEF_XC] = arrRegCoef2CanX[c];
            arrRegCoef2[COEF_XD] = arrRegCoef2CanX[d];

            arrRegCoef2[COEF_YA] = arrRegCoef2CanY[a];
            arrRegCoef2[COEF_YB] = arrRegCoef2CanY[b];
            arrRegCoef2[COEF_YC] = arrRegCoef2CanY[c];
            arrRegCoef2[COEF_YD] = arrRegCoef2CanY[d];

            return 0;
        }
        int CalINL2(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL2, float[] arrLin2, float[] arrInl2, int[] arrRegCoef2, float pPos, float nPos, float maxX, float minX, float maxY, float minY, float[] arrLin1, int INL)
        {

            // Normalized arrCoef2_1
            for (int i = 0; i < NUM_COEF2_1; i++)
            {
                sarrNorCoef2[i] = arrRegCoef2[i] / (AKM_Pow(2.0f, 7) - 1) * (pPos - nPos) + nPos;
            }

            // Normalized arrCoef2_2
            for (int i = 0; i < NUM_COEF2_2 - 1; i++)
            {
                if (arrRegCoef2[NUM_COEF2_1 + i] > AKM_Pow(2.0f, 6)) { sarrNorCoef2[NUM_COEF2_1 + i] = (arrRegCoef2[NUM_COEF2_1 + i] - AKM_Pow(2.0f, 7)) / AKM_Pow(2.0f, 10); }
                else { sarrNorCoef2[NUM_COEF2_1 + i] = arrRegCoef2[NUM_COEF2_1 + i] / AKM_Pow(2.0f, 10); }
            }
            sarrNorCoef2[COEF_GP] = arrRegCoef2[COEF_GP] + 1.0f;

            // Calclation of the compensation amount2 
            if (VT == 0) { CalLinGain2(norX, sarrNorCoef2, pPos, nPos, arrLin2); }
            if (VT == 1) { CalLinGain2(norY, sarrNorCoef2, pPos, nPos, arrLin2); }

            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                snorXLNC[i] = norX[i] - arrLin1[i] - arrLin2[i];
                sarrX13LNC[i] = (snorXLNC[i] - nPos) * AKM_Pow(2.0f, 13) / (pPos - nPos);
            }



            // -------------------------------------
            // Normalized Data 
            // -------------------------------------

            // Calculation of the INL after LNC2 at Normalized Target data
            if (VT == 0)
            {
                CalApprox(snorXLNC, norY, norX, snorYLNC, MAX_NUM_DATA, MAX_NUM_DATA);
                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl2[i] = arrIdeal[i] - snorYLNC[i];
                }
            }

            if (VT == 1)
            {
                for (int i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl2[i] = snorXLNC[i] - arrIdeal[i];
                }
            }

            // -------------------------------------
            // Target, Lens Position Data 
            // -------------------------------------

            // Caliculation of Lens data after LNC2
            if (INL == 1)
            {
                if (VT == 0)
                {
                    for (int i = 0; i < MAX_NUM_DATA; i++)
                    {
                        arrINL2[i] = arrIDEAL[i] - (arrY13[i] + (arrLin1[i] + arrLin2[i]) / (pPos - nPos) * (maxY - minY));
                    }
                }

                if (VT == 1)
                {
                    CalApprox(sarrX13LNC, arrY13, arrX13, sarrY13LNC, MAX_NUM_DATA, MAX_NUM_DATA);
                    for (int i = 0; i < MAX_NUM_DATA; i++)
                    {
                        arrINL2[i] = arrIDEAL[i] - sarrY13LNC[i];
                    }
                }

            }

            return 0;
        }
        int CalLinGain2(float[] inputX, float[] arrCoef2, float pPos, float nPos, float[] arrLin2)
        {
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                int k = 0;
                if (inputX[i] < arrCoef2[0])
                {
                    arrLin2[i] = arrCoef2[4] + (arrCoef2[5] - arrCoef2[4]) / (arrCoef2[0] - nPos) * (inputX[i] - nPos);
                }
                else if (inputX[i] < arrCoef2[1])
                {
                    k = 1;
                    arrLin2[i] = arrCoef2[4 + k] + (arrCoef2[5 + k] - arrCoef2[4 + k]) / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                }
                else if (inputX[i] < arrCoef2[2])
                {
                    k = 2;
                    arrLin2[i] = arrCoef2[4 + k] + (arrCoef2[5 + k] - arrCoef2[4 + k]) / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                }
                else if (inputX[i] < arrCoef2[3])
                {
                    k = 3;
                    arrLin2[i] = arrCoef2[4 + k] + (arrCoef2[5 + k] - arrCoef2[4 + k]) / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                }
                else
                {
                    k = 4;
                    arrLin2[i] = arrCoef2[4 + k] + (arrCoef2[5 + k] - arrCoef2[4 + k]) / (pPos - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                }
                arrLin2[i] *= arrCoef2[COEF_GP];
            }
            return 0;
        }
        int OptParamSeg(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float pPos, float nPos, float maxX, float minX, float[] arrIDEAL, float[] arrIdeal, float[] arrLin1, int[] arrRegCoef2, float[] arrINL2, float[] arrLin2, float[] arrInl2, int times)
        {
            // Segmentation 
            int[] numSeg = new int[NUM_COEF2_1];
            for (int i = 0; i < NUM_COEF2_1; i++)
            {
                numSeg[i] = 0;
            }

            int t = 0;
            for (int i = 0; i < MAX_NUM_DATA; i++)
            {
                if (arrX13[i] > arrRegCoef2[t] * AKM_Pow(2.0f, 6)) { numSeg[t] = i; t += 1; }
                if (t == 4) { break; }
            }

            int Start = 0, End = 0, Segment = 0;
            if (times == 0) { Start = 0; End = numSeg[0]; Segment = 0; }
            if (times == 1) { Start = numSeg[3]; End = MAX_NUM_DATA - 1; Segment = 4; }
            if (times == 2) { Start = numSeg[0]; End = numSeg[3]; Segment = 2; }

            // Part Search
            int bitSearch1 = 4;
            int bitSearch2 = BIT_COEF2 - bitSearch1;
            float resError = PRE_ERROR;
            float resIntError = PRE_ERROR;
            int minIndex = 0;
            float[] arrResError = new float[32];              // 2^bitSearch1 * 2

            for (int i = 0; i < MAX_NUM_DATA2; i++)
            {
                arrResError[i] = 0.0f;
            }

            // 1st Search: Rough 
            for (int i = 0; i < AKM_Pow(2.0f, bitSearch1); i++)
            {

                // Sort for monotonic increase 
                if (i < AKM_Pow(2.0f, bitSearch1 - 1)) { arrRegCoef2[Segment + NUM_COEF2_1 + 1] = i * (int)AKM_Pow(2.0f, bitSearch2) + (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
                else { arrRegCoef2[Segment + NUM_COEF2_1 + 1] = i * (int)AKM_Pow(2.0f, bitSearch2) - (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }

                for (int j = 0; j < AKM_Pow(2.0f, bitSearch1); j++)
                {
                    int num = i * (int)AKM_Pow(2.0f, bitSearch1) + j;

                    // Sort for monotonic increase 
                    if (j < AKM_Pow(2.0f, bitSearch1 - 1)) { arrRegCoef2[Segment + NUM_COEF2_1] = j * (int)AKM_Pow(2.0f, bitSearch2) + (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
                    else { arrRegCoef2[Segment + NUM_COEF2_1] = j * (int)AKM_Pow(2.0f, bitSearch2) - (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }

                    CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, 500, 0, arrLin1, 0);

                    for (int k = Start; k < End + 1; k++)
                    {
                        if (AKM_Abs(arrInl2[k]) > arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j]) { arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j] = AKM_Abs(arrInl2[k]); }
                    }
                    if (i != 0 && j != 0)
                    {
                        if (arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j] + arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j - 1] + arrResError[j] + arrResError[j - 1] < resError)
                        {
                            resError = arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j] + arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j - 1] + arrResError[j] + arrResError[j - 1];
                            minIndex = num;
                        }
                    }
                }

                for (int j = 0; j < AKM_Pow(2.0f, bitSearch1); j++)
                {
                    arrResError[j] = arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j];
                    arrResError[(int)AKM_Pow(2.0f, bitSearch1) + j] = 0.0f;
                }

            }

            int FirstSearchI = minIndex / (int)AKM_Pow(2.0f, bitSearch1);
            int FirstSearchJ = minIndex - FirstSearchI * (int)AKM_Pow(2.0f, bitSearch1);

            // 2nd Search: Fine 
            for (int i = (FirstSearchI - 1) * (int)AKM_Pow(2.0f, bitSearch2); i < FirstSearchI * AKM_Pow(2.0f, bitSearch2) + 1; i++)
            {
                // Sort for monotonic increase 
                if (i < AKM_Pow(2.0f, BIT_COEF2 - 1)) { arrRegCoef2[Segment + NUM_COEF2_1 + 1] = i + (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
                else { arrRegCoef2[Segment + NUM_COEF2_1 + 1] = i - (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }

                for (int j = (FirstSearchJ - 1) * (int)AKM_Pow(2.0f, bitSearch2); j < FirstSearchJ * AKM_Pow(2.0f, bitSearch2) + 1; j++)
                {
                    int num = i * (int)AKM_Pow(2.0f, BIT_COEF2) + j;
                    // Sort for monotonic increase 
                    if (j < AKM_Pow(2.0f, BIT_COEF2 - 1)) { arrRegCoef2[Segment + NUM_COEF2_1] = j + (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
                    else { arrRegCoef2[Segment + NUM_COEF2_1] = j - (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }

                    CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, 500, 0, arrLin1, 0);

                    float ResError = 0.0f;
                    float ResIntError = 0.0f;

                    for (int k = Start; k < End + 1; k++)
                    {
                        if (AKM_Abs(arrInl2[k]) > ResError) { ResError = AKM_Abs(arrInl2[k]); }
                        ResIntError += AKM_Abs(arrInl2[k]);
                    }

                    if (ResError < resError)
                    {
                        resError = ResError;
                        resIntError = ResIntError;
                        minIndex = num;
                    }

                    if (ResError == resError)
                    {
                        if (ResIntError < resIntError)
                        {
                            resIntError = ResIntError;
                            minIndex = num;
                        }
                    }

                }
            }

            arrRegCoef2[Segment + NUM_COEF2_1 + 1] = minIndex / (int)AKM_Pow(2.0f, BIT_COEF2);
            arrRegCoef2[Segment + NUM_COEF2_1] = minIndex - arrRegCoef2[Segment + NUM_COEF2_1 + 1] * (int)AKM_Pow(2.0f, BIT_COEF2);

            // Sort for Register
            for (int i = 0; i < 2; i++)
            {
                if (arrRegCoef2[Segment + NUM_COEF2_1 + i] < AKM_Pow(2.0f, BIT_COEF2 - 1)) { arrRegCoef2[Segment + NUM_COEF2_1 + i] += (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
                else { arrRegCoef2[Segment + NUM_COEF2_1 + i] -= (int)AKM_Pow(2.0f, BIT_COEF2 - 1); }
            }

            return 0;
        }
        int OptParamAll(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float pPos, float nPos, float maxX, float minX, float[] arrIDEAL, float[] arrIdeal, float[] arrLin1, int[] arrRegCoef2, float[] arrINL2, float[] arrLin2, float[] arrInl2)
        {
            //	float arrInl2		[MAX_NUM_DATA];
            float resError = PRE_ERROR;
            float resIntError = PRE_ERROR;
            float ResError = 0.0f;
            float ResIntError = 0.0f;
            int minIndex = 0;
            int yaIni, ybIni, ycIni, ydIni;
            int i, j, k, l;
            yaIni = arrRegCoef2[COEF_YA] - RANGE / 2;
            ybIni = arrRegCoef2[COEF_YB] - RANGE / 2;
            ycIni = arrRegCoef2[COEF_YC] - RANGE / 2;
            ydIni = arrRegCoef2[COEF_YD] - RANGE / 2;

            for (i = 0; i < RANGE; i++)
            {
                arrRegCoef2[COEF_YA] = yaIni + i;
                if (arrRegCoef2[COEF_YA] < 0) { arrRegCoef2[COEF_YA] = 0; }
                if (arrRegCoef2[COEF_YA] > AKM_Pow(2.0f, BIT_COEF2) - 1) { arrRegCoef2[COEF_YA] = (int)AKM_Pow(2.0f, BIT_COEF2) - 1; }

                for (j = 0; j < RANGE; j++)
                {
                    arrRegCoef2[COEF_YB] = ybIni + j;
                    if (arrRegCoef2[COEF_YB] < 0) { arrRegCoef2[COEF_YB] = 0; }
                    if (arrRegCoef2[COEF_YB] > AKM_Pow(2.0f, BIT_COEF2) - 1) { arrRegCoef2[COEF_YB] = (int)AKM_Pow(2.0f, BIT_COEF2) - 1; }

                    for (k = 0; k < RANGE; k++)
                    {
                        arrRegCoef2[COEF_YC] = ycIni + k;
                        if (arrRegCoef2[COEF_YC] < 0) { arrRegCoef2[COEF_YC] = 0; }
                        if (arrRegCoef2[COEF_YC] > AKM_Pow(2.0f, BIT_COEF2) - 1) { arrRegCoef2[COEF_YC] = (int)AKM_Pow(2.0f, BIT_COEF2) - 1; }

                        for (l = 0; l < RANGE; l++)
                        {
                            arrRegCoef2[COEF_YD] = ydIni + l;
                            if (arrRegCoef2[COEF_YD] < 0) { arrRegCoef2[COEF_YD] = 0; }
                            if (arrRegCoef2[COEF_YD] > AKM_Pow(2.0f, BIT_COEF2) - 1) { arrRegCoef2[COEF_YD] = (int)AKM_Pow(2.0f, BIT_COEF2) - 1; }

                            int num = i * (int)AKM_Pow(RANGE, 3) + j * (int)AKM_Pow(RANGE, 2) + k * (int)AKM_Pow(RANGE, 1) + l;

                            CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, maxX, minX, 500, 0, arrLin1, 0);

                            ResError = 0.0f;
                            ResIntError = 0.0f;

                            for (int m = 0; m < MAX_NUM_DATA; m++)
                            {

                                if (AKM_Abs(arrInl2[m]) > ResError) { ResError = AKM_Abs(arrInl2[m]); }
                                ResIntError += AKM_Abs(arrInl2[m]);
                            }

                            if (ResError < resError)
                            {
                                resError = ResError;
                                resIntError = ResIntError;
                                minIndex = num;
                            }

                            if (ResError == resError)
                            {
                                if (ResIntError < resIntError)
                                {
                                    resIntError = ResIntError;
                                    minIndex = num;
                                }
                            }

                        }
                    }
                }
            }

            i = minIndex / (int)AKM_Pow(RANGE, 3);
            j = (minIndex - i * (int)AKM_Pow(RANGE, 3)) / (int)AKM_Pow(RANGE, 2);
            k = (minIndex - i * (int)AKM_Pow(RANGE, 3) - j * (int)AKM_Pow(RANGE, 2)) / (int)AKM_Pow(RANGE, 1);
            l = (minIndex - i * (int)AKM_Pow(RANGE, 3) - j * (int)AKM_Pow(RANGE, 2) - k * (int)AKM_Pow(RANGE, 1));

            arrRegCoef2[COEF_YA] = yaIni + i;
            arrRegCoef2[COEF_YB] = ybIni + j;
            arrRegCoef2[COEF_YC] = ycIni + k;
            arrRegCoef2[COEF_YD] = ydIni + l;

            for (int n = COEF_YA; n < COEF_YD + 1; n++)
            {
                if (arrRegCoef2[n] < 0) { arrRegCoef2[n] = 0; }
                if (arrRegCoef2[n] > AKM_Pow(2.0f, BIT_COEF2) - 1) { arrRegCoef2[n] = (int)AKM_Pow(2.0f, BIT_COEF2) - 1; }
            }
            return 0;
        }
        int ConvReg(int[] arrRegCoef1, int[] arrRegCoef2, ref int[] linCoef)
        {
            linCoef[0] = arrRegCoef1[COEF_X3];
            linCoef[1] = arrRegCoef1[COEF_X2];
            linCoef[2] = arrRegCoef1[COEF_X1];
            linCoef[3] = arrRegCoef2[COEF_XA] * 2 + (arrRegCoef1[COEF_X0] / 16) % 2;
            linCoef[4] = arrRegCoef2[COEF_XB] * 2 + (arrRegCoef1[COEF_X0] / 8) % 2;
            linCoef[5] = arrRegCoef2[COEF_XC] * 2 + (arrRegCoef1[COEF_X0] / 4) % 2;
            linCoef[6] = arrRegCoef2[COEF_XD] * 2 + (arrRegCoef1[COEF_X0] / 2) % 2;
            linCoef[7] = arrRegCoef2[COEF_Y0] * 2 + (arrRegCoef1[COEF_X0]) % 2;
            linCoef[8] = arrRegCoef2[COEF_YA] * 2 + (arrRegCoef1[COEF_GF] / 4) % 2;
            linCoef[9] = arrRegCoef2[COEF_YB] * 2 + (arrRegCoef1[COEF_GF] / 2) % 2;
            linCoef[10] = arrRegCoef2[COEF_YC] * 2 + (arrRegCoef1[COEF_GF]) % 2;
            linCoef[11] = arrRegCoef2[COEF_YD] * 2 + (arrRegCoef2[COEF_GP] / 2) % 2;
            linCoef[12] = arrRegCoef2[COEF_YM] * 2 + (arrRegCoef2[COEF_GP]) % 2;

            return 0;
        }
    }
}

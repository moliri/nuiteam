using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmekFramework.Common.BasicTypes;
using System.Runtime.InteropServices;
using OmekFramework.Common.GeneralUtils;
using OmekFramework.Common.SensorImage;
using UnityEngine;

namespace UnitySensorImageRelated
{

    /// <summary>
    /// Provides static ImageData to Color array converters (as WriteableBitmap is very useful for WPF, the conversation is currently only to that format).
    /// </summary>
	public static class UnityImageConverters
    {

        private static readonly Color32 ZERO_COLOR = new Color(0, 0, 0, 0);
        private static readonly Color32 WHITE_COLOR = new Color32(255, 255, 255, 255);
        #region Defenitions for Depth Scaling
        /// <summary>
        /// Contains statistical pixel value information
        /// </summary>
        private class PixelValueBucket
        {
            public ulong m_sum;
            public double m_ssum;
            public uint m_amount;

            public void Clear()
            {
                m_sum = 0;
                m_ssum = 0;
                m_amount = 0;
            }
        }
        /// <summary>
        /// An array of backets, each containing data about the pixel data within it.
        /// </summary>
        private static PixelValueBucket[] m_bucketList;
        /// <summary>
        /// The amount of buckets to use when calculating the area where the buckets resides.
        /// </summary>
        private static readonly int BUCKET_AMOUNT = 10;
        #endregion
        #region Constructors
        static UnityImageConverters()
        {
             m_bucketList = new PixelValueBucket[BUCKET_AMOUNT];
			for (int i = 0; i < BUCKET_AMOUNT; ++i)
			{
				m_bucketList[i] = new PixelValueBucket();
			}
        }
        #endregion
        #region Public static converters
        /// <summary>
        /// Converts an ImageData type of the depth or color image to a color32 array and masks it according to the entered person mask.
        /// This function does not call Texture2D.apply. which should be called to apply the changes to the texture
        /// </summary>
        /// <param name="regImage">Contains the regular image data and format</param>
        /// <param name="maskImage">Contains the person mask image data and format</param>
        /// <param name="flipx">should the image be flipped horizontally</param>
        /// <param name="flipy">should the image be flipped vertically</param>
        /// <param name="pixels">The Color32 Array to write to. mast be in the right size for the wanted operation.
        /// <returns>ReturnCode indicating status of the run</returns>
        public static ReturnCode ConvertImageToMaskedColorArray(RegularImageData regImage, MaskImageData maskImage, bool flipx, bool flipy,  ref Color32[] pixels)
        {
            return ConvertImageToMaskedColorArray(regImage, maskImage, flipx, flipy, WHITE_COLOR, ref  pixels);
        }

        /// <summary>
        /// Converts an ImageData type of the depth or color image to a color32[] and masks it according to the entered person mask.
        /// This function does not call Texture2D.apply. which should be called to apply the changes to the texture
        /// </summary>
        /// <param name="regImage">Contains the regular image data and format</param>
        /// <param name="maskImage">Contains the person mask image data and format</param>
        /// <param name="flipx">should the image be flipped horizontally</param>
        /// <param name="flipy">should the image be flipped vertically</param>
        /// <param name="depthColor">what is the base color of a depth image</param>
        /// <param name="pixels">The Color32 Array to write to. mast be in the right size for the wanted operation.
        /// <returns>ReturnCode indicating status of the run</returns>
        public static ReturnCode ConvertImageToMaskedColorArray(RegularImageData regImage, MaskImageData maskImage, bool flipx,bool flipy, Color32 depthColor,ref Color32[] pixels)
        {
            // get the image format and data
            CommonDefines.ImageFormat iFormat;
            regImage.GetImageFormat(out iFormat);
            CommonDefines.ImageData imageData;
            ReturnCode rc = regImage.GetData(out imageData);
            if (rc.IsError())
            {
                return rc;
            }
            // get the mask format and data
            CommonDefines.ImageFormat maskFormat;
            maskImage.GetImageFormat(out maskFormat);
            CommonDefines.ImageData maskedData;
            rc = maskImage.GetData(out maskedData);
            if (rc.IsError())
            {
                return rc;
            }

            rc = VerifyImageFormat(iFormat, pixels);
            if (rc.IsError())
            {
                return rc;
            }
            
            int imagePos = 0;
            int pixelPos = 0;
            int bpp = iFormat.m_bpc;
            int bppTimeChannels = iFormat.m_channels * bpp;
            int imagePixelHeight = iFormat.m_height;
            int imagePixelWidth = iFormat.m_width;
            int formatWidthStep = iFormat.m_widthStep;

            int maskHeightRatio = iFormat.m_height / maskFormat.m_height;
            int maskWidthRatio = iFormat.m_width / maskFormat.m_width;
            int maskImageWidth = maskFormat.m_width;
            
            // parameters for normalizing
            ushort minVal1 = 0, maxVal1 = 0;
            ushort minVal2 = 0, maxVal2 = 0;
            float range1 = 0,range2 = 0;
            bool isGrayscale = (iFormat.m_channels == 1);
            bool isDepth = (regImage.GetType() == typeof(DepthImageData));           
            minVal1 = (ushort)regImage.MinPixelValue;
            maxVal1 = (ushort)regImage.MaxPixelValue;
            range1 = maxVal1 - minVal1;
            if (isDepth)
            {
                GetPixelRange((DepthImageData)regImage, maskImage, out minVal2, out maxVal2, out range2);
            }
            else // on regualr images we dont want to tint the color
            {
                depthColor = WHITE_COLOR;
            }

            // get the loop vlues - this handle fliping the image horizontally and verticlly
            int xStart, xDir, yStart, yDir;
            SetLoopVars(flipx, flipy, iFormat, out xStart, out xDir, out yStart, out yDir);

            // iterate through the image
            for (int yindex = 0; yindex != imagePixelHeight; ++yindex)
            {
                for (int xindex = 0; xindex != imagePixelWidth; ++xindex)
                {
                    int y = yStart + yDir * yindex;
                    int x = xStart + xDir * xindex;
                    // for each pixel get the coresponding pixel in the player mask
                    int maskPos = (y / maskHeightRatio) * maskImageWidth + (x / maskWidthRatio);
                    byte maskVal = maskedData.m_dataArr[maskPos];

                    // get the pixel in the image
                    imagePos = y * formatWidthStep + x * bppTimeChannels;
                    pixelPos = yindex * imagePixelWidth + xindex;
                    if (maskVal > 0)
                    {
                        // get the image value and normalize if needed
                        if (isGrayscale) 
                        {
                            // values are ushorts, get them from a byte[]
                            uint val1 = (uint)((imageData.m_dataArr[imagePos + 1] << 8) + imageData.m_dataArr[imagePos]);
                            // normalize according to the whole image
                            int val = (ushort)((val1 - minVal1) / (float)range1 * ushort.MaxValue);

                            
 
                            float relativeVal = (float)val / ushort.MaxValue;

                            // we get here only in depth, we normalize according to the specific person and clamp values to [0.1,1]
                            if (range2 != 0)
                            {
                                relativeVal = (maxVal2 - val) / ((float)range2);
                                relativeVal = (relativeVal < 0.1f) ? 0.1f : relativeVal;
                            }
                            else // in normal grayscale image we clamp values to [0,1]
                            {
								
                                relativeVal = (relativeVal <= 0) ? 1f : relativeVal;
                            }                            
                            relativeVal = (relativeVal > 1f) ? 1f : relativeVal;

                            pixels[pixelPos].a = depthColor.a;
                            pixels[pixelPos].r = (byte)(depthColor.r * relativeVal);
                            pixels[pixelPos].g = (byte)(depthColor.g * relativeVal);
                            pixels[pixelPos].b = (byte)(depthColor.b * relativeVal);
                            
                        }
                        else
                        {
                            pixels[pixelPos].a = 255;
                            pixels[pixelPos].r = imageData.m_dataArr[imagePos];
                            pixels[pixelPos].g = (imageData.m_dataArr[imagePos + 1]);
                            pixels[pixelPos].b = (imageData.m_dataArr[imagePos + 2]);
                        }
                    }
                    else
                    {
                        pixels[pixelPos] = ZERO_COLOR;
                    }

                }

            }
            return ReturnCode.OK_Status;
        }

        /// <summary>
        /// Converts an SensorImageData type of the depth or color image to a color32 array.        
        /// This function does not call Texture2D.apply. which should be called to apply the changes to the texture
        /// </summary>
        /// <param name="sensorImageData">Contains the sensor image data</param>
        /// /// <param name="flipx">should the image be flipped horizontally</param>
        /// <param name="flipy">should the image be flipped vertically</param>
        /// <param name="pixels">The Color32 Array to write to. mast be in the right size for the wanted operation.
        /// <returns>ReturnCode indicating status of the run</returns>
        public static ReturnCode ConvertImageToColorArray(RegularImageData sensorImageData, bool flipx, bool flipy, ref Color32[] pixels)
        {
            CommonDefines.ImageFormat iFormat;
            sensorImageData.GetImageFormat(out iFormat);
            CommonDefines.ImageData imageData;
            ReturnCode rc = sensorImageData.GetData(out imageData);
            if (rc.IsError())
            {
                return rc;
            }            
            rc = VerifyImageFormat(iFormat, pixels);
            if (rc.IsError())
            {
                return rc;
            }


            int imagePos = 0;
            int pixelPos = 0;
            int bpp = iFormat.m_bpc;
            int bppTimeChannels = iFormat.m_channels * bpp;
            int imagePixelHeight = iFormat.m_height;
            int imagePixelWidth = iFormat.m_width;
            int formatWidthStep = iFormat.m_widthStep;
            
            // convert from 2-bpp to 1-bpp
            ushort fromVal = 0, toVal = 0;
            float range = 0;
            bool isGrayscale = (iFormat.m_channels == 1);
            bool isDepth = (sensorImageData.GetType() == typeof(DepthImageData));
            if (isGrayscale)
            {
                fromVal = (ushort)sensorImageData.MinPixelValue;
                toVal = (ushort)sensorImageData.MaxPixelValue;
                range = toVal - fromVal;
            }
            // if we use depth image we want to reverse the grayscale - white for close pixels and black for far ones.
            if (isDepth)
            {
                fromVal = toVal;
                range *= -1;

            }
            int xStart, xDir, yStart, yDir;
            SetLoopVars(flipx, flipy, iFormat, out xStart, out xDir, out yStart, out yDir);

            for (int yindex = 0; yindex != imagePixelHeight; ++yindex)
            {
                for (int xindex = 0; xindex != imagePixelWidth; ++xindex)
                {
                    int y = yStart + yDir * yindex;
                    int x = xStart + xDir * xindex;

                    imagePos = y * formatWidthStep + x * bppTimeChannels;
                    pixelPos = yindex * imagePixelWidth + xindex;

                    if (isGrayscale)
                    {
                        if ((imagePos + 1) <= imageData.m_dataArr.Length)
                        {
                            int intVal = (imageData.m_dataArr[imagePos + 1] << 8) + imageData.m_dataArr[imagePos];
                            // if isDepth values of 0 are actually no data, not very close to camera
                            if (intVal <= 0)
                            {
                                pixels[pixelPos].r = pixels[pixelPos].g = pixels[pixelPos].b = 0;
                                pixels[pixelPos].a = 255;
                            }
                            else
                            {
                                float val = (intVal - fromVal) / range;
                                val = val < 0f ? 0f : val;
                                val = val > 1f ? 1f : val;

                                pixels[pixelPos].a = 255;
                                pixels[pixelPos].r = pixels[pixelPos].g = pixels[pixelPos].b = (byte)(255 * val);
                            }
                        }
                        
                    }
                    else
                    {
                        pixels[pixelPos].a = 255;
                        pixels[pixelPos].r = imageData.m_dataArr[imagePos];
                        pixels[pixelPos].g = (imageData.m_dataArr[imagePos + 1]);
                        pixels[pixelPos].b = (imageData.m_dataArr[imagePos + 2]);
                    }
                    

                }

            }
            return ReturnCode.OK_Status;
        }
        
        /// <summary>
        /// Converts an ImageData type of the person mask to a color32 array.
        /// This function does not call Texture2D.apply. which should be called to apply the changes to the texture
        /// </summary>
		/// <param name="maskImageData">Contains the mask image data</param>
        /// <param name="maskColor">the color to set the mask to</param>
        /// <param name="pixels">The color array to write the person mask on. 
        /// It must fit the image size, or else an error is returned</param>        
        /// <returns>ReturnCode indicating status of the run</returns>
		public static ReturnCode ConvertPersonMaskToColorArray(MaskImageData maskImageData, Color32 maskColor,bool flipx,bool flipy, ref Color32[] pixels)
        {           
            CommonDefines.ImageData imageData;
            ReturnCode rc = maskImageData.GetData(out imageData);
            if (rc.IsError())
            {
                return rc;
            }
            CommonDefines.ImageFormat iFormat;
            maskImageData.GetImageFormat(out iFormat);
             
            rc = VerifyImageFormat(iFormat, pixels);
            if (rc.IsError())
            {
                return rc;
            }
            int xStart, xDir, yStart, yDir;
            SetLoopVars(flipx, flipy, iFormat, out xStart,  out xDir, out yStart, out yDir);
            
            for (int y = 0; y != iFormat.m_height; ++y)
            {
                for (int x = 0; x != iFormat.m_width; ++x)
                {
                    int pos = (yStart + yDir * y) * iFormat.m_widthStep + xStart + xDir * x;
                    int pixelPos = y * iFormat.m_widthStep + x;
                    pixels[pixelPos] = (imageData.m_dataArr[pos] > 0) ? maskColor : ZERO_COLOR;
                }
            }
            return ReturnCode.OK_Status;
        }


        #endregion
        #region Method used for sacling Depth Image
       

        /// <summary>
        /// Gets the pixel range for the depth normalization according to mean and two stds around it, and by throwing away all data buckets which 
		/// don't include atleast as much they would need acocording to a uniform distribution according to the bucket amount.
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="shortImage">The short image pointer</param>
        /// <param name="minVal">(output) min value</param>
        /// <param name="maxVal">(output) max value</param>
        /// <param name="range">(output) range between the min and max values</param>
        private static void GetPixelRange(DepthImageData depthImage, MaskImageData maskImage, out ushort minVal, out ushort maxVal, out float range)
        {
           
			for (int i = 0; i < BUCKET_AMOUNT; ++i)
			{
				m_bucketList[i].Clear();
			}
            CommonDefines.ImageFormat depthFormat, maskFormat;
            depthImage.GetImageFormat(out depthFormat);
            maskImage.GetImageFormat(out maskFormat);
            int height = depthFormat.m_height;
            int width = depthFormat.m_width;
            int formatWidthStep = depthFormat.m_widthStep;
            int bppTimeChannels = depthFormat.m_bpc  * depthFormat.m_channels;
            int maskHeightRatio = height / maskFormat.m_height;
            int maskWidthRatio = width / maskFormat.m_width;
            int maskWidth = maskFormat.m_width;
            ushort globalMin = (ushort)depthImage.MinPixelValue;
            ushort globalMax = (ushort)depthImage.MaxPixelValue;
            float origRange = (float)(globalMax - globalMin);

            CommonDefines.ImageData maskImageData;
            maskImage.GetData(out maskImageData);
            byte[] maskData = maskImageData.m_dataArr;
            CommonDefines.ImageData depthImageData;
            depthImage.GetData(out depthImageData);
            byte[] depthData = depthImageData.m_dataArr;


			float bucketSize = ushort.MaxValue / BUCKET_AMOUNT;
			int totalAmount = 0;

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int maskPos = (y / maskHeightRatio) * maskWidth + (x / maskWidthRatio);
                    byte maskVal = maskData[maskPos];

                    int imagePos = y * formatWidthStep + x * bppTimeChannels;

                    if (maskVal > 0)
                    {

                        int val = (int)((depthData[imagePos + 1] << 8) + depthData[imagePos]);
						float floatVal = (val - globalMin) / origRange;
						if (floatVal > 1)
						{
							floatVal = (floatVal > 1) ? 1 : floatVal;
						}
						floatVal = (floatVal < 0) ? 0 : floatVal;
						val =(int)(floatVal * ushort.MaxValue);						
                        if (val > 0)
                        {
                            //int bucketIndex = (int)Math.Min((val / bucketSize), (BUCKET_AMOUNT - 1));
                            int bucketIndex = (int)((val / bucketSize) < (BUCKET_AMOUNT - 1) ? (val / bucketSize) : (BUCKET_AMOUNT - 1));
                            ++m_bucketList[bucketIndex].m_amount;
                            m_bucketList[bucketIndex].m_sum += (ulong)val;
                            m_bucketList[bucketIndex].m_ssum += (val / 100f) * (val / 100f);
                            ++totalAmount;
                        }
                    }
                }
            }

			int minBucketAmount = totalAmount / BUCKET_AMOUNT;
			double cleanedSsum = 0;
			ulong cleanedSum = 0;
			uint cleanedAmount = 0;
			for (int i = 0; i < BUCKET_AMOUNT; ++i)
			{
				if (m_bucketList[i].m_amount > minBucketAmount)
				{
					cleanedAmount += m_bucketList[i].m_amount;
					cleanedSum += m_bucketList[i].m_sum;
					cleanedSsum += m_bucketList[i].m_ssum;
				}
			}

            double average = 0;
            double std = 0;
			if (cleanedAmount > 0)
            {
				average = cleanedSum / (cleanedAmount * 1.0f);
				std = Math.Sqrt(((cleanedSsum / (cleanedAmount * 1.0f))) * 10000f - (average * average));
            }


			minVal = (ushort)Math.Max((average - 2f * std), ushort.MinValue);
			maxVal = (ushort)Math.Min((average + 2f * std), ushort.MaxValue);
            //maxVal = (ushort)Math.Min((average + 10), ushort.MaxValue);
            //minVal = (ushort)Math.Max((average - 10), ushort.MinValue);
			range = (float)(maxVal - minVal);            
        }
    		
        #endregion
        # region Private helper functions

    
        /// <summary>
        /// Verifies that the entered WriteableBitmap is of the correct format according to the specified ImageFormat.
        /// Returns an error code if it is not of the correct format.
        /// </summary>
        /// <param name="iFormat">The ImageFormat to verify the WriteableBitmap against</param>
        /// <param name="img">The WriteableBitmap to check</param>
        /// <returns>ReturnCode which is OK_Status if it is the correct format. Otherwise the correct error is returned</returns>
		private static ReturnCode VerifyImageFormat(CommonDefines.ImageFormat iFormat, Color32[] pixels)
		{
            if (pixels == null && pixels.Length != iFormat.m_width * iFormat.m_height)
			{
				return new ReturnCode(ReturnCode.FrameworkReturnCodes.Image_Format_Incompatible);
			}
			return ReturnCode.OK_Status;
		}

        /// <summary>
        /// used to set loop variable to help flip images
        /// </summary>
        /// <param name="flipx">shoud we flip the image arounf x horizontaly</param>
        /// <param name="flipy">shoud we flip the image arounf x verticaly</param>
        /// <param name="iFormat">the image format</param>
        /// <param name="xStart">(out) the start value for x</param>
        /// <param name="xDir">(out) the direction of x values</param>
        /// <param name="yStart">(out) the start value for y</param>
        /// <param name="yDir">(out) the direction of y values</param>
        private static void SetLoopVars(bool flipx, bool flipy, CommonDefines.ImageFormat iFormat, out int xStart, out int xDir, out int yStart, out int yDir)
        {
            if (flipx)
            {
                xStart = iFormat.m_width - 1;
                xDir = -1;
            }
            else
            {
                xStart = 0;
                xDir = 1;
            }

            if (flipy)
            {
                yStart = iFormat.m_height - 1;
                yDir = -1;
            }
            else
            {
                yStart = 0;
                yDir = 1;
            }
        }
        #endregion
    }
}

#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Contour", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueContourNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Line Number", DefaultValue = 10, IsSingle = true)]
		public ISpread<int> FInLineNumber;
		
		[Input("Range", DefaultValue = 1, IsSingle = true)]
		public ISpread<double> FInRange;
		
		[Input("Mesh Number", DefaultValue = 50, IsSingle = true)]
		public ISpread<int> FInMeshNumber;
		
		[Input("Delta", DefaultValue = 1, IsSingle = true)]
		public ISpread<double> FInDelta;
		
		[Output("From")]
		public ISpread<Vector2D> FOutFrom;
		[Output("To")]
		public ISpread<Vector2D> FOutTo;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		private const double flowVelocity = 1;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutFrom.SliceCount = 0;
			FOutTo.SliceCount = 0;
			
			double maxP = flowVelocity * FInRange[0];
			double minP = -maxP;
			double dp0 = (maxP - minP) / FInLineNumber[0];
			
			double pp = 0;
			double x1, y1, x2, y2;
			x1 = y1 = x2 = y2 = 0;
			
			for (int k = 0; k < FInLineNumber[0]; k++)
			{
				pp = minP + (k+1) * dp0;
				
				for (int i=0; i<FInMeshNumber[0]; i++)
				{
					for (int j=0; j<FInMeshNumber[0]; j++)
					{
						double[] p = new double[6];
						double[] x = new double[6];
						double[] y = new double[6];
						
						// separate triangle mesh
						int mn = FInMeshNumber[0] + 1;
						p[0] = FInput[mn*i+j]; x[0] = i * FInDelta[0]; y[0] = j * FInDelta[0];
						p[1] = FInput[mn*i+(j+1)]; x[1] = i * FInDelta[0]; y[1] = (j+1) * FInDelta[0];
						p[2] = FInput[mn*(i+1)+(j+1)]; x[2] = (i+1) * FInDelta[0]; y[2] = (j+1) * FInDelta[0];
						p[3] = FInput[mn*(i+1)+j]; x[3] = (i+1) * FInDelta[0]; y[3] = j * FInDelta[0];
						p[4] = p[0]; x[4] = x[0]; y[4] = y[0]; // same index 0
						// center
						p[5] = (p[0] + p[1] + p[2] + p[3]) / 4.0;
						x[5] = (x[0] + x[1] + x[2] + x[3]) / 4.0;
						y[5] = (y[0] + y[1] + y[2] + y[3]) / 4.0;
						
						for (int m=0; m<4; m++)
						{
							x1= -10.0; y1 = -10.0;
							
							if ((p[m] <= pp && pp < p[m+1]) || (p[m] > pp && pp >= p[m+1]))
							{
								x1 = x[m] + (x[m+1] - x[m]) * (pp - p[m]) / (p[m+1] - p[m]);
								y1 = y[m] + (y[m+1] - y[m]) * (pp - p[m]) / (p[m+1] - p[m]);
							}
							if ((p[m] <= pp && pp <= p[5]) || (p[m] >= pp && pp >= p[5]))
							{
								if (x1 < 0.0)
								{
									x1 = x[m] + (x[5] - x[m]) * (pp - p[m]) / (p[5] - p[m]);
			  						y1 = y[m] + (y[5] - y[m]) * (pp - p[m]) / (p[5] - p[m]);
								}
								else
								{
									x2 = x[m] + (x[5] - x[m]) * (pp - p[m]) / (p[5] - p[m]);
									y2 = y[m] + (y[5] - y[m]) * (pp - p[m]) / (p[5] - p[m]);
									
									FOutFrom.Add(new Vector2D(-1.0 + x1, -1.0 + y1));
									FOutFrom.SliceCount++;
									FOutTo.Add(new Vector2D(-1.0 + x2, -1.0 + y2));
									FOutTo.SliceCount++;
								}
							}
							if ((p[m+1] <= pp && pp <= p[5]) || (p[m+1] >= pp && pp >= p[5]))
							{
								if (x1 < 0.0)
								{
									x1 = x[m+1] + (x[5] - x[m+1]) * (pp - p[m+1]) / (p[5] - p[m+1]);
									y1 = y[m+1] + (y[5] - y[m+1]) * (pp - p[m+1]) / (p[5] - p[m+1]);
								}
								else
								{
									x2 = x[m+1] + (x[5] - x[m+1]) * (pp - p[m+1]) / (p[5] - p[m+1]);
									y2 = y[m+1] + (y[5] - y[m+1]) * (pp - p[m+1]) / (p[5] - p[m+1]);
									
									FOutFrom.Add(new Vector2D(-1.0 + x1, -1.0 + y1));
									FOutFrom.SliceCount++;
									FOutTo.Add(new Vector2D(-1.0 + x2, -1.0 + y2));
									FOutTo.SliceCount++;
								}
							}
						}
					}
				}
			}
		}
	}
}

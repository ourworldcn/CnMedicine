using CnMedicineServer.Bll;
using OW.Data.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CnMedicineTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        float? f;
        private void Form1_Load(object sender, EventArgs e)
        {
            List<(string, decimal)> lst = new List<(string, decimal)>();
            EntityUtility.FileListInArrayWithPower(",1101(sd),1202*3,1301（手段）*.5,5202-1,4107*2.5,1703。", lst, 1);
            var t1 = typeof(List<int>);
            var t2 = typeof(List<short>);
            ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            lockSlim.EnterWriteLock();
            lockSlim.EnterReadLock();
            lockSlim.ExitWriteLock();
            if (null == f)
                f = 0.5f;
            var t = OwInitializer.BeginInitialize();
            Matrix4x4 m1 = new Matrix4x4(
                1, 2, 3, 4,
                2, 3, 4, 5,
                3, 4, 5, 6,
                4, 5, 6, 7);
            Matrix4x4 m2 = new Matrix4x4(
                0, 2, 3, 4,
                2, 3, 4, 5,
                3, 4, 5, 6,
                4, 5, 6, 7);
            int rank0 = 100;    //200基准30ms,1000基准5s
            Random rnd = new Random();
            var mm = new MmoCanges<FastChangePositionBase>();
            List<(Vector3, FastChangePositionBase)> dispare = new List<(Vector3, FastChangePositionBase)>();
            List<(Vector3, FastChangePositionBase)> moved = new List<(Vector3, FastChangePositionBase)>();
            List<(Vector3, FastChangePositionBase)> distroy = new List<(Vector3, FastChangePositionBase)>();
            FastChangePositionBase[] arys = new FastChangePositionBase[2000];
            for (int i = 0; i < 2000; i++)
            {

                var tmp = new FastChangePositionBase()
                {
                    Position = new Vector3((float)rnd.NextDouble() * 400, (float)rnd.NextDouble() * 400, 0)
                };
                arys[i] = tmp;
                mm.Change(tmp, 1, dispare, moved, distroy);
                dispare.Clear();
                moved.Clear();
                distroy.Clear();
            }
            var bm = ObjectPool.Default;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                Parallel.For(0, 20000, new ParallelOptions() { MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * 1), }, c =>
                {
                    using (var ch = ObjectPool.TakeObject<ClearHelper>())
                    {
                        var tmp = arys[rnd.Next(2000)];
                        tmp.Position = new Vector3(tmp.Position.X + (float)rnd.NextDouble() * 2 - 1f, tmp.Position.Y + (float)rnd.NextDouble() * 2 - 1f, 0);
                        var l1 = ch.TakeObjectAndReturn<List<(Vector3, FastChangePositionBase)>>();
                        var l2 = ch.TakeObjectAndReturn<List<(Vector3, FastChangePositionBase)>>();
                        var l3 = ch.TakeObjectAndReturn<List<(Vector3, FastChangePositionBase)>>();
                        mm.Change(tmp, 40, l1, l2, l3);
                        l1.Clear();
                        l2.Clear();
                        l3.Clear();
                    }
                });
            }
            finally
            {
                sw.Stop();
            }
            var op = ObjectPool.Default;
            MessageBox.Show($"{sw.Elapsed.ToString()}s,{mm.sd}");

        }

        public class SingleMatrix
        {
            public SingleMatrix(int rowCount, int columnCount)
            {
                _RowCount = rowCount;
                _ColumnCount = columnCount;
                _Datas = new float[rowCount * columnCount];
            }

            private float[] _Datas;

            public float[] Datas
            {
                get { return _Datas; }
                set { _Datas = value; }
            }

            private int _RowCount;
            public int RowCount { get => _RowCount; set => _RowCount = value; }

            private int _ColumnCount;
            public int ColumnCount { get => _ColumnCount; set => _ColumnCount = value; }

            public int Count { get => _ColumnCount * _RowCount; }

            public static void Mult(SingleMatrix left, SingleMatrix right, ref SingleMatrix result)
            {
                UnsafeMult(left, right, ref result);
            }

            public unsafe static void UnsafeMult(SingleMatrix left, SingleMatrix right, ref SingleMatrix result)
            {
                if (left.ColumnCount != right.RowCount)
                    throw new ArgumentException();
                if (null == result)
                {
                    result = new SingleMatrix(left.RowCount, right.ColumnCount);
                }
                else if (left.RowCount != result.RowCount || right.ColumnCount != result.ColumnCount)
                    throw new ArgumentException();
                int count = left.ColumnCount;
                int strade = right.ColumnCount;
                fixed (float* l = &left._Datas[0], r = &right._Datas[0], o = &result._Datas[0])
                {
                    if ((long)left.RowCount * left.ColumnCount * (left.ColumnCount - 1) * right.ColumnCount < 1e9 || left.RowCount < Environment.ProcessorCount)
                        InnerMult(l, left.RowCount, left.ColumnCount, r, right.ColumnCount, o);
                    else
                    {
                        var lpL = l;
                        var lpR = r;
                        var lpO = o;
                        var pResult = Parallel.For(0, left.RowCount, c =>
                        {
                            InnerMult(lpL + c * left.ColumnCount, 1, left.ColumnCount, lpR, right.ColumnCount, lpO + c * right.ColumnCount);
                        });
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="left"></param>
            /// <param name="rowCount"></param>
            /// <param name="columnCount"></param>
            /// <param name="right"></param>
            /// <param name="rightColumnCount"></param>
            /// <param name="result"></param>
            unsafe static private void InnerMult(float* left, int rowCount, int columnCount, float* right, int rightColumnCount, float* result)
            {
                var lpL = left;
                var lpR = right;
                var lpO = result;
                var strade = rightColumnCount;
                for (int i = rowCount - 1; i >= 0; i--)
                {
                    for (int j = rightColumnCount - 1; j >= 0; j--)
                    {
                        float tmpF = 0;
                        for (int k = columnCount - 1; k >= 0; k--)
                        {
                            tmpF += *lpL * *lpR;
                            lpL++;
                            lpR += strade;
                        }
                        *lpO = tmpF; lpO++;
                        lpL -= columnCount;
                        lpR -= columnCount * strade; lpR++;
                    }
                    lpL += columnCount;
                    lpR = right;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<(string, decimal)> lst = new List<(string, decimal)>();
            EntityUtility.FillValueTuples("白芍 9 百合 -18 乌药6", lst);
        }
    }
}

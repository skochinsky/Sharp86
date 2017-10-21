using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    [TestClass]
    public class OpCodeTests68 : CPUUnitTests
    {
        [TestInitialize]
        public void Setup()
        {
            _accessedPort = 0;
            _portValue = 0;
        }

        [TestMethod]
        public void push_Ib()
        {
            sp = 0x1000;
            emit("push -1");
            step();

            Assert.AreEqual(sp, 0x0FFE);
            Assert.AreEqual(ReadWord(ss, sp), 0xFFFF);  // byte will be sign extended
        }

        [TestMethod]
        public void push_Iv()
        {
            sp = 0x1000;
            emit("push 01ffh");
            step();

            Assert.AreEqual(sp, 0x0FFE);
            Assert.AreEqual(ReadWord(ss, sp), 0x01ff);
        }

        [TestMethod]
        public void imul_Gv_Ev_Ib()
        {
            ax = 0;
            bx = 10;
            emit("imul ax,bx,20");
            step();
            Assert.AreEqual(ax, 200);

            bx = 0xFFFF;
            emit("imul ax,bx,10");
            step();
            Assert.AreEqual(ax, 0xFFF6);
        }

        [TestMethod]
        public void imul_Gv_Ev_Iv()
        {
            ax = 0;
            bx = 10;
            emit("imul ax,bx,200");
            step();
            Assert.AreEqual(ax, 2000);

            bx = 0xFFFF;
            emit("imul ax,bx,1000");
            step();
            Assert.AreEqual(ax, unchecked((ushort)(short)-1000));
        }


        ushort _accessedPort;
        ushort _portValue;


        List<ushort> _writtenValues;

        public override ushort ReadPortWord(ushort port)
        {
            _accessedPort = port;
            ushort val = _portValue;
            _portValue++;
            return val;
        }

        public override void WritePortWord(ushort port, ushort value)
        {
            _accessedPort = port;
            _portValue = value;
            _writtenValues.Add(value);
        }

        [TestMethod]
        public void insb()
        {
            _portValue = 0x12;

            di = 0x1000;
            dx = 0x5678;
            emit("insb");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(di, 0x1001);
            Assert.AreEqual(ReadByte(ds, 0x1000), 0x12);
        }

        [TestMethod]
        public void rep_insb()
        {
            _portValue = 0x12;

            di = 0x1000;
            dx = 0x5678;
            cx = 5;
            emit("rep insb");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(di, 0x1005);
            Assert.AreEqual(cx, 0);
            for (int i=0; i<5; i++)
            {
                Assert.AreEqual(ReadByte(ds, (ushort)(0x1000 + i)), (byte)(0x12 + i));
            }
        }

        [TestMethod]
        public void rep_insb_cx0()
        {
            _portValue = 0x12;

            di = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep insb");
            step();

            Assert.AreEqual(_accessedPort, 0);
            Assert.AreEqual(di, 0x1000);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void insw()
        {
            _portValue = 0x1234;

            di = 0x1000;
            dx = 0x5678;
            emit("insw");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(di, 0x1002);
            Assert.AreEqual(ReadWord(ds, 0x1000), 0x1234);
        }

        [TestMethod]
        public void rep_insw()
        {
            _portValue = 0x1245;

            di = 0x1000;
            dx = 0x5678;
            cx = 5;
            emit("rep insw");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(di, 0x100A);
            Assert.AreEqual(cx, 0);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(ReadWord(ds, (ushort)(0x1000 + i * 2)), (ushort)(0x1245 + i));
            }
        }

        [TestMethod]
        public void rep_insw_cx0()
        {
            _portValue = 0x1245;

            di = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep insw");
            step();

            Assert.AreEqual(_accessedPort, 0);
            Assert.AreEqual(di, 0x1000);
            Assert.AreEqual(cx, 0);
        }

        [TestMethod]
        public void outsb()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            WriteByte(ds, si, 0x12);
            emit("outsb");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(si, 0x1001);
            Assert.AreEqual(_writtenValues.Count, 1);
            Assert.AreEqual(_writtenValues[0], 0x12);
        }

        [TestMethod]
        public void rep_outsb()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            cx = 5;
            for (int i = 0; i < 5; i++)
            {
                WriteByte(ds, (ushort)(si + i), (byte)(0x12 + i));
            }
            emit("rep outsb");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(si, 0x1005);
            Assert.AreEqual(_writtenValues.Count, 5);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(_writtenValues[i], (byte)(0x12 + i));
            }
        }

        [TestMethod]
        public void rep_outsb_cx0()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep outsb");
            step();

            Assert.AreEqual(_accessedPort, 0);
            Assert.AreEqual(si, 0x1000);
            Assert.AreEqual(_writtenValues.Count, 0);
        }

        [TestMethod]
        public void outsw()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            WriteWord(ds, si, 0x1234);
            emit("outsw");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(si, 0x1002);
            Assert.AreEqual(_writtenValues.Count, 1);
            Assert.AreEqual(_writtenValues[0], 0x1234);
        }

        [TestMethod]
        public void rep_outsw()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            cx = 5;
            for (int i = 0; i < 5; i++)
            {
                WriteWord(ds, (ushort)(si + i * 2), (ushort)(0x1234 + i));
            }
            emit("rep outsw");
            step();

            Assert.AreEqual(_accessedPort, 0x5678);
            Assert.AreEqual(si, 0x100A);
            Assert.AreEqual(_writtenValues.Count, 5);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(_writtenValues[i], (ushort)(0x1234 + i));
            }
        }

        [TestMethod]
        public void rep_outsw_cx0()
        {
            _writtenValues = new List<ushort>();

            si = 0x1000;
            dx = 0x5678;
            cx = 0;
            emit("rep outsw");
            step();

            Assert.AreEqual(_accessedPort, 0);
            Assert.AreEqual(si, 0x1000);
            Assert.AreEqual(_writtenValues.Count, 0);
        }

    }
}

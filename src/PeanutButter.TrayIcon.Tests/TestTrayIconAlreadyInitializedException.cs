﻿using System;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestTrayIconAlreadyInitializedException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(TrayIconAlreadyInitializedException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut)
                .To.Inherit<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new TrayIconAlreadyInitializedException();

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Equal("This instance of the TrayIcon has already been initialized");
        }
    }

}
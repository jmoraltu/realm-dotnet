﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using NUnit.Framework;
using Realms;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class RemoveTests
    {
        protected string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void RemoveSucceedsTest()
        {
            // Arrange
            Person p1 = null, p2 = null, p3 = null;
            _realm.Write(() =>
            {
                p1 = _realm.CreateObject<Person>(); p1.FirstName = "A";
                p2 = _realm.CreateObject<Person>(); p2.FirstName = "B";
                p3 = _realm.CreateObject<Person>(); p3.FirstName = "C";
            });

            // Act
            _realm.Write(() => _realm.Remove(p2));

            // Assert
            //Assert.That(!p2.InRealm);

            var allPeople = _realm.All<Person>().ToList();
            Assert.That(allPeople, Is.EquivalentTo(new List<Person> { p1, p3 }));
        }


        [Test]
        public void RemoveOutsideTransactionShouldFail()
        {
            // Arrange
            Person p = null;
            _realm.Write(() => p = _realm.CreateObject<Person>());

            // Act and assert
            Assert.Throws<RealmOutsideTransactionException>(() => _realm.Remove(p) );
        }

        [Test]
        public void RemoveRangeCanRemoveSpecificObjects()
        {
            // Arrange
            _realm.Write(() =>
            {
                var p1 = _realm.CreateObject<Person>();
                p1.FirstName = "deletable person #1";
                p1.IsInteresting = false;

                var p2 = _realm.CreateObject<Person>();
                p2.FirstName = "person to keep";
                p2.IsInteresting = true;

                var p3 = _realm.CreateObject<Person>();
                p3.FirstName = "deletable person #2";
                p3.IsInteresting = false;
            });

            // Act
            _realm.Write(() => _realm.RemoveRange<Person>(((RealmResults<Person>)_realm.All<Person>().Where(p => !p.IsInteresting))));

            // Assert
            Assert.That(_realm.All<Person>().ToList().Select(p => p.FirstName).ToArray(),
                Is.EqualTo(new[] { "person to keep" }));
        }

        [Test]
        public void RemoveAllRemovesAllObjectsOfAGivenType() 
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.CreateObject<Person>();
                _realm.CreateObject<Person>();
                _realm.CreateObject<Person>();

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(3));
            });

            // Act
            _realm.Write(() => _realm.RemoveAll<Person>());

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void RemoveAllObjectsShouldClearTheDatabase()
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.CreateObject<Person>();
                _realm.CreateObject<Person>();
                _realm.CreateObject<AllTypesObject>();

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(2));
                Assert.That(_realm.All<AllTypesObject>().Count(), Is.EqualTo(1));
            });

            // Act
            _realm.Write(() => _realm.RemoveAll());

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
            Assert.That(_realm.All<AllTypesObject>().Count(), Is.EqualTo(0));

        }
    }
}

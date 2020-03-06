using Formula1.Core;
using Formula1.Core.Entities;
using Formula1.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula1.PersistenceTests
{
    [TestClass()]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            // Build the ApplicationDbContext 
            //  - with InMemory-DB
            return new ApplicationDbContext(
              new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options);
        }

        [TestMethod]
        public async Task D01_FirstDataAccessTest()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                Team team = new Team
                {
                    Name = "Red Bull",
                    Nationality = "Austria"
                };
                dbContext.Teams.Add(team);
                await dbContext.SaveChangesAsync();
            }
            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                var firstOrDefault = dbContext.Teams.FirstOrDefault(t => t.Name == "Red Bull");
                Assert.IsNotNull(firstOrDefault, "Zumindest ein Team muss in DB sein");
                Assert.AreEqual("Red Bull", firstOrDefault.Name);
            }
        }

        [TestMethod]
        public async Task D02_TeamWithDrivers()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                Team team_RedBull = new Team
                {
                    Name = "Red Bull",
                    Nationality = "Austria",
                    Drivers = new List<Driver>
                    {
                        new Driver
                        {
                            FirstName = "Max",
                            LastName = "Verstappen"
                        },
                        new Driver
                        {
                            FirstName = "Alexander",
                            LastName = "Albon"
                        }
                    }
                };

                Team team_Mercedes = new Team
                {
                    Name = "Mercedes",
                    Nationality = "Deutschland",
                    Drivers = new List<Driver>
                    {
                        new Driver
                        {
                            FirstName = "Lewis",
                            LastName = "Hamilton"
                        },
                        new Driver
                        {
                            FirstName = "Valtteri",
                            LastName = "Bottas"
                        }
                    }
                };

                Team team_Ferrari = new Team
                {
                    Name = "Ferrari",
                    Nationality = "Italien",
                    Drivers = new List<Driver>
                    {
                        new Driver
                        {
                            FirstName = "Sebastian",
                            LastName = "Vettel"
                        },
                        new Driver
                        {
                            FirstName = "Charles",
                            LastName = "Leclerc"
                        }
                    }
                };

                            
                dbContext.Teams.Add(team_RedBull);
                dbContext.Teams.Add(team_Mercedes);
                dbContext.Teams.Add(team_Ferrari);

                await dbContext.SaveChangesAsync();
            }
            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                Assert.AreEqual(3, dbContext.Teams.Count());
                Assert.AreEqual(6, dbContext.Drivers.Count());
            }
        }

        [TestMethod]
        public async Task D03_LoadResults_Get_Teams_Drivers_Races()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                var results = ImportController.LoadResultsFromXmlIntoCollections().ToList();

                dbContext.Results.AddRange(results);

                await dbContext.SaveChangesAsync();
            }
            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                Assert.AreEqual(11, dbContext.Teams.Count());
                Assert.AreEqual(24, dbContext.Drivers.Count());
                Assert.AreEqual(21, dbContext.Races.Count());
            }
        }

        [TestMethod]
        public async Task D04_LoadRaces_GetRacesInGermany()
        {
            string dbName = Guid.NewGuid().ToString();

            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                var races = ImportController.LoadRacesFromRacesXml().ToList();

                dbContext.Races.AddRange(races);

                await dbContext.SaveChangesAsync();
            }
            using (ApplicationDbContext dbContext = GetDbContext(dbName))
            {
                Assert.AreEqual(1, dbContext.Races.Count(r => r.Country.Equals("Germany")));
            }
        }
    }
}
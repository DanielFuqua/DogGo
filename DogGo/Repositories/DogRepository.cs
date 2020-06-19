﻿using DogGo.Models;
using Doggo.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogGo.Repositories
{
    public class DogRepository
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public DogRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        public List<Dog> GetAllDogs()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], OwnerId, Breed, Notes, ImageUrl
                        FROM Dog
                    ";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Dog> dogs = new List<Dog>();
                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = ReaderHelpers.GetNullableString(reader, "Notes"),
                            ImageUrl = ReaderHelpers.GetNullableString(reader, "ImageUrl")


                        };

                        dogs.Add(dog);
                    }

                    reader.Close();

                    return dogs;
                }
            }
        }


        public List<Dog> GetDogsByOwnerId(int ownerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Breed, Notes, ImageUrl, OwnerId 
                        FROM Dog
                        WHERE OwnerId = @ownerId
                    ";

                    cmd.Parameters.AddWithValue("@ownerId", ownerId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog d = new Dog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = ReaderHelpers.GetNullableString(reader, "Notes"),
                            ImageUrl = ReaderHelpers.GetNullableString(reader, "ImageUrl")
                        };


                        // We could also null check optional columns like this

                        //if (reader.IsDBNull(reader.GetOrdinal("Notes")) == false)
                        //{
                        //    d.Notes = reader.GetString(reader.GetOrdinal("Notes"));
                        //}

                        //if (reader.IsDBNull(reader.GetOrdinal("ImageUrl")) == false)
                        //{
                        //    d.ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
                        //}

                        dogs.Add(d);
                    }

                    reader.Close();

                    return dogs;
                }
            }
        }

        public void AddDog(Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    INSERT INTO Dog ([Name], OwnerId, Breed, Notes, ImageUrl)
                    OUTPUT INSERTED.ID
                    VALUES (@name, @ownerId, @breed, @notes, @imageUrl);
                ";

                    cmd.Parameters.AddWithValue("@name", dog.Name);
                    cmd.Parameters.AddWithValue("@ownerId", dog.OwnerId);
                    cmd.Parameters.AddWithValue("@breed", dog.Breed);
                    if(dog.Notes != null)
                    {
                        cmd.Parameters.AddWithValue("@notes", dog.Notes);
                    } 
                    else
                    {
                        cmd.Parameters.AddWithValue("@notes", DBNull.Value);

                    }
                    if (dog.ImageUrl != null)
                    {
                        cmd.Parameters.AddWithValue("@imageUrl", dog.ImageUrl);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@imageUrl", DBNull.Value);
                    }

                   

                    int id = (int)cmd.ExecuteScalar();

                    dog.Id = id;
                }
            }
        }
    }
}

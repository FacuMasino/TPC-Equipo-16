﻿using System;
using System.Net;
using DataAccessLayer;
using DomainModelLayer;
using UtilitiesLayer;

namespace BusinessLogicLayer
{
    public class PeopleManager
    {
        private DataAccess _dataAccess = new DataAccess();
        private AddressesManager _addressesManager = new AddressesManager();

        public Person Read(int personId)
        {
            Person person = new Person();

            try
            {
                _dataAccess.SetProcedure("SP_Read_Person");
                _dataAccess.SetParameter("@PersonId", personId);
                _dataAccess.ExecuteRead();

                if (_dataAccess.Reader.Read())
                {
                    person.PersonId = personId;
                    person.IsActive = (bool)_dataAccess.Reader["IsActive"];
                    person.FirstName = _dataAccess.Reader["FirstName"]?.ToString();
                    person.FirstName = person.FirstName ?? "";
                    person.LastName = _dataAccess.Reader["LastName"]?.ToString();
                    person.LastName = person.LastName ?? "";
                    person.TaxCode = _dataAccess.Reader["TaxCode"]?.ToString();
                    person.TaxCode = person.TaxCode ?? "";
                    person.Phone = _dataAccess.Reader["Phone"]?.ToString();
                    person.Phone = person.Phone ?? "";
                    person.Email = _dataAccess.Reader["Email"]?.ToString();
                    person.Email = person.Email ?? "";

                    if (_dataAccess.Reader.IsDBNull(_dataAccess.Reader.GetOrdinal("Birth"))) // hack
                    {
                        person.Birth = DateTime.MinValue;
                    }
                    else
                    {
                        person.Birth = _dataAccess.Reader.GetDateTime(
                            _dataAccess.Reader.GetOrdinal("Birth")
                        );
                    }

                    person.Address.Id =
                        _dataAccess.Reader["AddressId"] as int? ?? person.Address.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }

            person.Address = _addressesManager.Read(person.Address.Id);

            return person;
        }

        public int Add(Person person)
        {
            _addressesManager.HandleAddressId(person.Address);

            int personId = 0;

            try
            {
                _dataAccess.SetProcedure("SP_Add_Person");
                SetParameters(person);
                personId = _dataAccess.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }

            return personId;
        }

        public void Edit(Person person)
        {
            _addressesManager.HandleAddressId(person.Address);

            try
            {
                _dataAccess.SetProcedure("SP_Edit_Person");
                _dataAccess.SetParameter("@PersonId", person.PersonId);
                SetParameters(person);
                _dataAccess.ExecuteAction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }
        }

        public void DeleteLogically(Person person)
        {
            try
            {
                _dataAccess.SetProcedure("SP_Delete_Person_Logically");
                _dataAccess.SetParameter("@PersonId", person.PersonId);
                _dataAccess.ExecuteAction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }
        }

        public void Delete(Person person, bool isLogicalDeletion = true)
        {
            if (isLogicalDeletion == true)
            {
                DeleteLogically(person);
                return;
            }

            try
            {
                _dataAccess.SetQuery("delete from People where PersonId = @PersonId");
                _dataAccess.SetParameter("@PersonId", person.PersonId);
                _dataAccess.ExecuteAction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }
        }

        public int GetId(Person person)
        {
            if (person == null)
            {
                return 0;
            }

            return GetId(person.Email);
        }

        public int GetId(string email)
        {
            int personId = 0;

            try
            {
                _dataAccess.SetProcedure("SP_Get_Person_Id");
                _dataAccess.SetParameter("@Email", email);
                _dataAccess.ExecuteRead();

                if (_dataAccess.Reader.Read())
                {
                    personId = (int)_dataAccess.Reader["PersonId"];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dataAccess.CloseConnection();
            }

            return personId;
        }

        public void HandlePersonId(Person person)
        {
            int foundId = GetId(person);

            if (foundId == 0)
            {
                person.PersonId = Add(person);
            }
            else if (foundId == person.PersonId)
            {
                Edit(person);
            }
            else
            {
                person.PersonId = foundId;
            }
        }

        private void SetParameters(Person person)
        {
            _dataAccess.SetParameter("@IsActive", person.IsActive);
            _dataAccess.SetParameter("@FirstName", person.FirstName);
            _dataAccess.SetParameter("@LastName", person.LastName);

            if (person.TaxCode != null)
            {
                _dataAccess.SetParameter("@TaxCode", person.TaxCode);
            }
            else
            {
                _dataAccess.SetParameter("@TaxCode", DBNull.Value);
            }

            if (person.Phone != null)
            {
                _dataAccess.SetParameter("@Phone", person.Phone);
            }
            else
            {
                _dataAccess.SetParameter("@Phone", DBNull.Value);
            }

            _dataAccess.SetParameter("@Email", person.Email);

            if (person.Birth != null)
            {
                _dataAccess.SetParameter("@Birth", person.Birth);
            }
            else
            {
                _dataAccess.SetParameter("@Birth", DBNull.Value);
            }

            if (person.Address == null || person.Address.Id == 0)
            {
                _dataAccess.SetParameter("@AddressId", DBNull.Value);
            }
            else
            {
                _dataAccess.SetParameter("@AddressId", person.Address.Id);
            }
        }
    }
}

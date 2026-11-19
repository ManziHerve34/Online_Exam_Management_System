using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using OnlineExamsSystem.Data;
using OnlineExamsSystem.Models;

namespace OnlineExamsSystem.Services
{
    public class VenueService
    {
        public List<Venue> GetAllVenues(bool activeOnly = false)
        {
            var list = new List<Venue>();
            string sql = "SELECT * FROM `venues` ";
            if (activeOnly)
                sql += " WHERE `is_active` = 1 ";
            sql += " ORDER BY `venue_name` ASC, `city` ASC;";

            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            foreach (DataRow row in dt.Rows)
                list.Add(MapVenue(row));

            return list;
        }

        public Venue GetVenueById(int venueId)
        {
            string sql = "SELECT * FROM `venues` WHERE `venue_id` = @VenueId;";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new MySqlParameter("@VenueId", venueId));
            if (dt.Rows.Count > 0)
                return MapVenue(dt.Rows[0]);
            return null;
        }

        public bool CreateVenue(Venue venue, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(venue.VenueName) || string.IsNullOrWhiteSpace(venue.Address) || string.IsNullOrWhiteSpace(venue.City))
            {
                error = "Venue name, address, and city are required.";
                return false;
            }

            try
            {
                string sql = @"
                INSERT INTO `venues`
                (`venue_name`, `address`, `city`, `room_number`, `total_capacity`, `contact_person`, `contact_phone`, `is_active`)
                VALUES
                (@Name, @Address, @City, @Room, @Capacity, @ContactPerson, @Phone, @Active);";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@Name", venue.VenueName.Trim()),
                    new MySqlParameter("@Address", venue.Address.Trim()),
                    new MySqlParameter("@City", venue.City.Trim()),
                    new MySqlParameter("@Room", (object)venue.RoomNumber?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Capacity", venue.TotalCapacity > 0 ? venue.TotalCapacity : 30),
                    new MySqlParameter("@ContactPerson", (object)venue.ContactPerson?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)venue.ContactPhone?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Active", venue.IsActive ? 1 : 0));

                AuditService.LogAction(SessionManager.CurrentUserId, "VENUE_CREATED", $"Created venue: {venue.VenueName} in {venue.City}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool UpdateVenue(Venue venue, out string error)
        {
            error = string.Empty;
            try
            {
                string sql = @"
                UPDATE `venues`
                SET `venue_name` = @Name,
                    `address` = @Address,
                    `city` = @City,
                    `room_number` = @Room,
                    `total_capacity` = @Capacity,
                    `contact_person` = @ContactPerson,
                    `contact_phone` = @Phone,
                    `is_active` = @Active
                WHERE `venue_id` = @VenueId;";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new MySqlParameter("@Name", venue.VenueName.Trim()),
                    new MySqlParameter("@Address", venue.Address.Trim()),
                    new MySqlParameter("@City", venue.City.Trim()),
                    new MySqlParameter("@Room", (object)venue.RoomNumber?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Capacity", venue.TotalCapacity > 0 ? venue.TotalCapacity : 30),
                    new MySqlParameter("@ContactPerson", (object)venue.ContactPerson?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)venue.ContactPhone?.Trim() ?? DBNull.Value),
                    new MySqlParameter("@Active", venue.IsActive ? 1 : 0),
                    new MySqlParameter("@VenueId", venue.VenueId));

                AuditService.LogAction(SessionManager.CurrentUserId, "VENUE_UPDATED", $"Updated venue ID {venue.VenueId}: {venue.VenueName}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool DeleteVenue(int venueId, out string error)
        {
            error = string.Empty;
            try
            {
                // Check if referenced in schedules
                string checkSql = "SELECT COUNT(*) FROM `exam_schedules` WHERE `venue_id` = @VenueId;";
                long count = Convert.ToInt64(DatabaseHelper.ExecuteScalar(checkSql, new MySqlParameter("@VenueId", venueId)));
                if (count > 0)
                {
                    string softDelete = "UPDATE `venues` SET `is_active` = 0 WHERE `venue_id` = @VenueId;";
                    DatabaseHelper.ExecuteNonQuery(softDelete, new MySqlParameter("@VenueId", venueId));
                    AuditService.LogAction(SessionManager.CurrentUserId, "VENUE_DEACTIVATED", $"Venue ID {venueId} has associated schedules; deactivated instead of deleted.");
                    return true;
                }

                string deleteSql = "DELETE FROM `venues` WHERE `venue_id` = @VenueId;";
                DatabaseHelper.ExecuteNonQuery(deleteSql, new MySqlParameter("@VenueId", venueId));
                AuditService.LogAction(SessionManager.CurrentUserId, "VENUE_DELETED", $"Deleted venue ID {venueId}.");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private Venue MapVenue(DataRow row)
        {
            return new Venue
            {
                VenueId = Convert.ToInt32(row["venue_id"]),
                VenueName = row["venue_name"].ToString(),
                Address = row["address"].ToString(),
                City = row["city"].ToString(),
                RoomNumber = row["room_number"]?.ToString(),
                TotalCapacity = Convert.ToInt32(row["total_capacity"]),
                ContactPerson = row["contact_person"]?.ToString(),
                ContactPhone = row["contact_phone"]?.ToString(),
                IsActive = Convert.ToBoolean(row["is_active"])
            };
        }
    }
}

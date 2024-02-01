using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using insurance_backend.Enums;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Models.Request.Product
{
	public class PropertyInsuranceProductCreateRequest : ProductBase
	{
		public double HousePerMeterSqaureCoefficient { get; set; }

		public double FlatPerMeterSqaureCoefficient { get; set; }

		public double GaragePerMeterSqaureCoefficient { get; set; }

		public double EquipmentCoefficient { get; set; }

		public double LiabilityCoefficient { get; set; }
	}
}

using insurance_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace insurance_backend.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Int32)]
        public int Price { get; set; }

        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("description")]
        [BsonRepresentation(BsonType.String)]
        public string Description { get; set; }

        [BsonElement("companyName")]
        [BsonRepresentation(BsonType.String)]
        public string CompanyName { get; set; }

        [BsonElement("companyLogo")]
        [BsonRepresentation(BsonType.String)]
        public string CompanyLogo { get; set; }

        [BsonElement("category")]
        [BsonRepresentation(BsonType.Int32)]
        public ProductCategory Category { get; set; }
    }
}

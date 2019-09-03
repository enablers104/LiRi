
using System.Linq;

namespace Microsoft.BotBuilderSamples
{
    // Extends the partial FlightBooking class with methods and properties that simplify accessing entities in the luis results
    public partial class LiriStockModel
    {
        /// <summary>
        /// Gets from entities.
        /// </summary>
        /// <value>
        /// From entities.
        /// </value>
        public (string From, string Branch) FromEntities
        {
            get
            {
                var fromValue = Entities?._instance?.From?.FirstOrDefault()?.Text;
                var fromBranchValue = Entities?.From?.FirstOrDefault()?.Branch?.FirstOrDefault()?.FirstOrDefault();
                return (fromValue, fromBranchValue);
            }
        }

        /// <summary>
        /// Gets to entities.
        /// </summary>
        /// <value>
        /// To entities.
        /// </value>
        public (string To, string Branch) ToEntities
        {
            get
            {
                var toValue = Entities?._instance?.To?.FirstOrDefault()?.Text;
                var toBranchValue = Entities?.To?.FirstOrDefault()?.Branch?.FirstOrDefault()?.FirstOrDefault();
                return (toValue, toBranchValue);
            }
        }
        
        // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
        // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
        public string TravelDate => Entities.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault()?.Split('T')[0];

        public string Color => Entities.Color?.FirstOrDefault()?.Split('T')[0];

        public string Garment => Entities.Garment?.FirstOrDefault()?.Split('T')[0];

        public string Brand => Entities.Brand?.FirstOrDefault()?.Split('T')[0];

        public string Size => Entities.Size?.FirstOrDefault()?.Split('T')[0];
    }
}

using Xunit;
using POE_Part2_PROG6212.Models;

namespace CMCSTests
{
    public class ClaimTests
    {
        // =====================================================
        // TEST 1: TotalAmount calculation
        // =====================================================
        [Fact]
        public void TotalAmount_ShouldCalculateCorrectly()
        {
            // arrange
            var claim = new Claim
            {
                HoursWorked = 20,
                HourlyRate = 670
            };

            // act
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // assert
            Assert.Equal(13400, claim.TotalAmount);
        }

        // =====================================================
        // TEST 2: Notes stored properly
        // =====================================================
        [Fact]
        public void Notes_ShouldStoreTextCorrectly()
        {
            var claim = new Claim
            {
                Notes = "Additional Notes submitted."
            };

            Assert.Equal("Additional Notes submitted.", claim.Notes);
        }

        // =====================================================
        // TEST 3: ClaimDocument file properties
        // =====================================================
        [Fact]
        public void Document_FileProperties_ShouldStoreCorrectValues()
        {
            var document = new ClaimDocument
            {
                FileName = "invoice.pdf",
                RelativePath = "/uploads/invoice.pdf"
            };

            Assert.Equal("invoice.pdf", document.FileName);
            Assert.Equal("/uploads/invoice.pdf", document.RelativePath);
        }

        // =====================================================
        // TEST 4: Status change
        // =====================================================
        [Fact]
        public void ClaimStatus_ShouldUpdateCorrectly()
        {
            var claim = new Claim
            {
                Status = ClaimStatus.Submitted
            };

            // simulate approval
            claim.Status = ClaimStatus.Approved;
            Assert.Equal(ClaimStatus.Approved, claim.Status);

            // simulate rejection
            claim.Status = ClaimStatus.Rejected;
            Assert.Equal(ClaimStatus.Rejected, claim.Status);
        }

        // =====================================================
        // TEST 5: TotalAmount small calculation
        // =====================================================
        [Fact]
        public void TotalAmount_ShouldReturnCorrectValue()
        {
            var claim = new Claim
            {
                HoursWorked = 5,
                HourlyRate = 200
            };

            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            Assert.Equal(1000, claim.TotalAmount);
        }
    }
}

using System.Diagnostics.Contracts;

namespace PwnedPasswords.FileClient.Loader
{
    internal class VerificationResult
    {
        public long HitsAboveThreshold { get; }
        public long HitsBelowThreshold { get; }
        public long MissesAboveThreshold { get; }
        public long MissesBelowThreshold { get; }

        public long PasswordsAboveThreshold => HitsAboveThreshold + MissesAboveThreshold;
        public long PasswordsBelowThreshold => HitsBelowThreshold + MissesBelowThreshold;

        public VerificationResult(long hitsAboveThreshold, long hitsBelowThreshold, long missesAboveThreshold, long missesBelowThreshold)
        {
            HitsAboveThreshold = hitsAboveThreshold;
            HitsBelowThreshold = hitsBelowThreshold;
            MissesAboveThreshold = missesAboveThreshold;
            MissesBelowThreshold = missesBelowThreshold;
        }

        public override string ToString()
        {
            return
                $"Above threshold: {HitsAboveThreshold} hits, {MissesAboveThreshold} misses. Below threshold: {HitsBelowThreshold} hits, {MissesBelowThreshold} misses";
        }

        public static VerificationResult Empty { get; } = new VerificationResult(0, 0, 0, 0);

        [Pure]
        public VerificationResult Add(VerificationResult other)
        {
            return new VerificationResult(
                HitsAboveThreshold + other.HitsAboveThreshold,
                HitsBelowThreshold + other.HitsBelowThreshold,
                MissesAboveThreshold + other.MissesAboveThreshold,
                MissesBelowThreshold + other.MissesBelowThreshold);
        }
    }
}
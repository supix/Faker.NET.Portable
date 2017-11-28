﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Faker.Caching;
using Faker.Extensions;

namespace Faker
{
    /// <summary>
    ///   A collection of Code related resources.
    /// </summary>
    /// <threadsafety static="true" />
    public static class Code
    {
        /// <summary>
        ///   Generates a EAN Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated EAN has a valid checksum or not
        /// </param>
        /// <returns>The generated EAN</returns>
        /// <remarks>
        ///   Description of the EAN standard is at
        ///   <see href="https://en.wikipedia.org/wiki/International_Article_Number" /> Checksum
        ///   routines are at <see href="http://www.isbn-check.de/servejs.pl?src=isbnfront.perlserved.js" />
        /// </remarks>
        public static string EAN(bool validChecksum = true)
        {
            int[] v = new int[12];
            for (int i = 0; i < 12; i++)
            {
                v[i] = RandomNumber.Next(0, 10);
            }

            var prefix = string.Join(string.Empty, v);

            return prefix + ComputeChecksum(validChecksum, v);
        }

        /// <summary>
        ///   Generates an Italian Fiscal Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated Fiscal Code has a valid checksum or not
        /// </param>
        /// <returns>The generated Fiscal Code</returns>
        /// <remarks>Description of the Fiscal Code standard is at https://en.wikipedia.org/wiki/Italian_fiscal_code_card</remarks>
        public static string FiscalCode(bool validChecksum = true)
        {
            var birthday = Date.Birthday();
            return FiscalCode(null, null, birthday, validChecksum);
        }

        /// <summary>
        ///   Generates an Italian Fiscal Code
        /// </summary>
        /// <param name="minAge">Minimum age of the holder</param>
        /// <param name="maxAge">Maximum age of the holder</param>
        /// <returns>The generated Fiscal Code</returns>
        /// <remarks>Description of the Fiscal Code standard is at https://en.wikipedia.org/wiki/Italian_fiscal_code_card</remarks>
        public static string FiscalCode(int minAge, int maxAge)
        {
            var birthday = Date.Birthday(minAge, maxAge);
            return FiscalCode(null, null, birthday, true);
        }

        /// <summary>
        ///   Generates an Italian Fiscal Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated Fiscal Code has a valid checksum or not
        /// </param>
        /// <param name="minAge">Minimum age of the holder</param>
        /// <param name="maxAge">Maximum age of the holder</param>
        /// <returns>The generated Fiscal Code</returns>
        /// <remarks>Description of the Fiscal Code standard is at https://en.wikipedia.org/wiki/Italian_fiscal_code_card</remarks>
        public static string FiscalCode(bool validChecksum, int minAge, int maxAge)
        {
            var birthday = Date.Birthday(minAge, maxAge);
            return FiscalCode(null, null, birthday, validChecksum);
        }

        /// <summary>
        ///   Generates an Italian Fiscal Code
        /// </summary>
        /// <param name="lastName">Last name of the holder</param>
        /// <param name="firstName">First name of the holder</param>
        /// <param name="birthday">Birthday of the holder</param>
        /// <param name="validChecksum">
        ///   Indicates whether the generated Fiscal Code has a valid checksum or not
        /// </param>
        /// <returns>The generated Fiscal Code</returns>
        /// <remarks>Description of the Fiscal Code standard is at https://en.wikipedia.org/wiki/Italian_fiscal_code_card</remarks>
        public static string FiscalCode(string lastName, string firstName, DateTime birthday, bool validChecksum = true)
        {
            char[] monthChars = { 'A', 'B', 'C', 'D', 'E', 'H', 'L', 'M', 'P', 'R', 'S', 'T' };

            if (string.IsNullOrWhiteSpace(lastName))
            {
                lastName = Name.Last();
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                firstName = Name.First();
            }

            var male = RandomNumber.Next(2) == 0; // even probability to be male or female

            var sb = new StringBuilder();
            sb.Append(GetFiscalCodeSqueezedName(lastName, false));
            sb.Append(GetFiscalCodeSqueezedName(firstName, true));
            sb.Append((birthday.Year % 100).ToString("00", CultureInfo.InvariantCulture));
            sb.Append(monthChars[birthday.Month - 1]);
            sb.Append((birthday.Day + (male ? 0 : 40)).ToString("00", CultureInfo.InvariantCulture));
            sb.Append(ResourceCollectionCacher.GetArray(PropertyHelper.GetProperty(() => Resources.FiscalCode_TownCodes.Codes)).Random());

            var checksum = ComputeChecksumFiscalCode(sb.ToString(), validChecksum);
            sb.Append(checksum);

            return sb.ToString();
        }

        /// <summary>
        ///   Generates an ISBN-10 Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated ISBN has a valid checksum or not
        /// </param>
        /// <returns>The generated ISBN-10</returns>
        /// <remarks>
        ///   Description of the ISBN standard is at
        ///   <see href="https://en.wikipedia.org/wiki/International_Standard_Book_Number" />
        ///   Checksum routines are at <see href="http://www.isbn-check.de/servejs.pl?src=isbnfront.perlserved.js" />
        /// </remarks>
        public static string ISBN10(bool validChecksum = true)
        {
            int[] v = new int[9];
            for (int i = 0; i < 9; i++)
            {
                v[i] = RandomNumber.Next(0, 10);
            }

            var checksum = ComputeChecksumIsbn10(v);

            if (!validChecksum)
            {
                checksum = (checksum + 1) % 11; // set wrong checksum
            }

            var prefix = string.Join(string.Empty, v);

            return prefix + (checksum < 10 ? checksum.ToString(CultureInfo.InvariantCulture) : "X");
        }

        /// <summary>
        ///   Generates an ISBN-13 Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated ISBN has a valid checksum or not
        /// </param>
        /// <returns>The generated ISBN-13</returns>
        /// <remarks>
        ///   Description of the ISBN standard is at
        ///   <see href="https://en.wikipedia.org/wiki/International_Standard_Book_Number" />
        ///   Checksum routines are at <see href="http://www.isbn-check.de/servejs.pl?src=isbnfront.perlserved.js" />
        /// </remarks>
        public static string ISBN13(bool validChecksum = true)
        {
            int[] v = new int[12];

            // ISBN13 starts with "978" or "979"
            v[0] = 9;
            v[1] = 7;
            v[2] = RandomNumber.Next(8, 10);
            for (int i = 3; i < 12; i++)
            {
                v[i] = RandomNumber.Next(10);
            }

            var prefix = string.Join(string.Empty, v);

            return prefix + ComputeChecksum(validChecksum, v);
        }

        /// <summary>
        ///   Generates a 10 digit NPI (National Provider Identifier issued to health care providers
        ///   in the United States)
        /// </summary>
        /// <returns>The generated NPI</returns>
        /// <remarks>
        ///   Description of the NPI standard is at
        ///   https://en.wikipedia.org/wiki/National_Provider_Identifier. The NPI has replaced the
        ///   unique physician identification number (UPIN).
        /// </remarks>
        public static string NPI()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(RandomNumber.Next(10));
            }

            return sb.ToString();
        }

        /// <summary>
        ///   Generates a Singaporean National Registration Identity Card (NRIC) for holder who is
        ///   born between specified ages.
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated NRIC has a valid checksum or not
        /// </param>
        /// <param name="minAge">Minimum age of the holder</param>
        /// <param name="maxAge">Maximum age of the holder</param>
        /// <returns>The generated NRIC</returns>
        /// <remarks>
        ///   Description of the NRIC standard is at
        ///   <see href="https://en.wikipedia.org/wiki/National_Registration_Identity_Card" />. This
        ///   ID was issued for the first time in 1965 in Singapore. See also: <see href="https://github.com/stympy/faker/blob/master/lib/faker/code.rb#L32" />
        /// </remarks>
        public static string NRIC(bool validChecksum = true, int minAge = 18, int maxAge = 65)
        {
            var birthYear = Date.Birthday(minAge, maxAge).Year;
            var prefix = birthYear < 2000 ? 'S' : 'T';
            var v = new int[7];
            for (int i = 0; i < 7; i++)
            {
                v[i] = RandomNumber.Next(10);
            }

            var checksum = ComputeChecksumNric(v, prefix, validChecksum);

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", prefix, string.Join(string.Empty, v), checksum);
        }

        /// <summary>
        ///   Generates a RUT Code
        /// </summary>
        /// <param name="validChecksum">
        ///   Indicates whether the generated RUT has a valid checksum or not
        /// </param>
        /// <returns>The generated RUT</returns>
        /// <remarks>
        ///   Description of the RUT standard is at
        ///   <see href="https://en.wikipedia.org/wiki/National_identification_number#Chile" />
        ///   Checksum routines are at <see href="http://www.vesic.org/english/blog-eng/net/verifying-chilean-rut-code-tax-number/" />
        /// </remarks>
        public static string RUT(bool validChecksum = true)
        {
            int[] v = new int[8];

            for (int i = 0; i < 8; i++)
            {
                v[i] = RandomNumber.Next(10);
            }

            var checksum = ComputeChecksumRut(v);

            if (!validChecksum)
            {
                checksum = (checksum + 1) % 11; // set wrong checksum
            }

            var prefix = string.Join(string.Empty, v);

            return prefix + (checksum < 10 ? checksum.ToString(CultureInfo.InvariantCulture) : "K");
        }

        private static int ComputeChecksum(bool validChecksum, int[] digits)
        {
            var checksum = ComputeChecksumEan(digits); // ISBN13 is an EAN

            if (!validChecksum)
            {
                checksum = (checksum + 1) % 10; // set wrong checksum
            }

            return checksum;
        }

        private static int ComputeChecksumEan(int[] digits)
        {
            var sum = 0;
            for (int i = 0; i < 12; i += 2)
            {
                sum += digits[i];
            }

            for (int i = 1; i < 12; i += 2)
            {
                sum += 3 * digits[i];
            }

            return (10 - (sum % 10)) % 10;
        }

        private static char ComputeChecksumFiscalCode(string prefix, bool validChecksum)
        {
            var oddMap = new Dictionary<char, int>
            {
                { '0', 1 },
                { '1', 0 },
                { '2', 5 },
                { '3', 7 },
                { '4', 9 },
                { '5', 13 },
                { '6', 15 },
                { '7', 17 },
                { '8', 19 },
                { '9', 21 },
                { 'A', 1 },
                { 'B', 0 },
                { 'C', 5 },
                { 'D', 7 },
                { 'E', 9 },
                { 'F', 13 },
                { 'G', 15 },
                { 'H', 17 },
                { 'I', 19 },
                { 'J', 21 },
                { 'K', 2 },
                { 'L', 4 },
                { 'M', 18 },
                { 'N', 20 },
                { 'O', 11 },
                { 'P', 3 },
                { 'Q', 6 },
                { 'R', 8 },
                { 'S', 12 },
                { 'T', 14 },
                { 'U', 16 },
                { 'V', 10 },
                { 'W', 22 },
                { 'X', 25 },
                { 'Y', 24 },
                { 'Z', 23 }
            };

            var evenMap = new Dictionary<char, int>
            {
                { '0', 0 },
                { '1', 1 },
                { '2', 2 },
                { '3', 3 },
                { '4', 4 },
                { '5', 5 },
                { '6', 6 },
                { '7', 7 },
                { '8', 8 },
                { '9', 9 },
                { 'A', 0 },
                { 'B', 1 },
                { 'C', 2 },
                { 'D', 3 },
                { 'E', 4 },
                { 'F', 5 },
                { 'G', 6 },
                { 'H', 7 },
                { 'I', 8 },
                { 'J', 9 },
                { 'K', 10 },
                { 'L', 11 },
                { 'M', 12 },
                { 'N', 13 },
                { 'O', 14 },
                { 'P', 15 },
                { 'Q', 16 },
                { 'R', 17 },
                { 'S', 18 },
                { 'T', 19 },
                { 'U', 20 },
                { 'V', 21 },
                { 'W', 22 },
                { 'X', 23 },
                { 'Y', 24 },
                { 'Z', 25 }
            };

            int total = 0;
            for (int i = 0; i < 15; i += 2)
            {
                total += oddMap[prefix[i]];
            }

            for (int i = 1; i < 15; i += 2)
            {
                total += evenMap[prefix[i]];
            }

            if (!validChecksum)
            {
                total++; // set wrong checksum
            }

            return (char)('A' + (total % 26));
        }

        private static int ComputeChecksumIsbn10(int[] digits)
        {
            int sum = 0;

            for (var i = 0; i < 9; i++)
            {
                sum += (10 - i) * digits[i];
            }

            sum *= 10;

            return sum % 11;
        }

        private static char ComputeChecksumNric(int[] digits, char prefix, bool validChecksum)
        {
            int[] weights = { 2, 7, 6, 5, 4, 3, 2 };
            var total = 0;
            for (int i = 0; i < 7; i++)
            {
                total += digits[i] * weights[i];
            }

            if (prefix == 'T')
            {
                total += 4;
            }

            if (!validChecksum)
            {
                total++; // set wrong checksum
            }

            char[] checksumChars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'Z', 'J' };

            return checksumChars[10 - (total % 11)];
        }

        private static int ComputeChecksumRut(int[] digits)
        {
            int[] coefs = { 3, 2, 7, 6, 5, 4, 3, 2 };

            int sum = 0;

            for (int i = 0; i < 8; i++)
            {
                sum += coefs[i] * digits[i];
            }

            return (11 - (sum % 11)) % 11;
        }

        /// <summary>
        ///   This method applies the rule giving the consonants and vowels extracted by the name,
        ///   according to the algorithm.
        /// </summary>
        /// <param name="name">The name to process</param>
        /// <param name="isFirstName">true, in case of first names</param>
        /// <returns>The squeezed name</returns>
        private static string GetFiscalCodeSqueezedName(string name, bool isFirstName)
        {
            var sb = new StringBuilder();
            var normalizedName = name.ToUpperInvariant();
            var regex = new Regex("[^A-Z]");
            normalizedName = regex.Replace(normalizedName, string.Empty);

            // manages firstname special case (first names having more than 3 consonants -> the 2nd
            // is skipped)
            var consonantToSkipIdx = -1;
            if (isFirstName)
            {
                var consonantCount = 0;
                for (int i = 0; i < normalizedName.Length; i++)
                {
                    if (!IsVowel(normalizedName[i]))
                    {
                        consonantCount++;
                        if (consonantCount == 2)
                        {
                            consonantToSkipIdx = i;
                        }
                    }
                }

                if (consonantCount <= 3)
                {
                    consonantToSkipIdx = -1;
                }
            }

            // add consonants
            for (int i = 0; i < normalizedName.Length; i++)
            {
                if (!IsVowel(normalizedName[i]) && (i != consonantToSkipIdx))
                {
                    sb.Append(normalizedName[i]);
                    if (sb.Length == 3)
                    {
                        return sb.ToString();
                    }
                }
            }

            // add vowels
            for (int i = 0; i < normalizedName.Length; i++)
            {
                if (IsVowel(normalizedName[i]))
                {
                    sb.Append(normalizedName[i]);
                    if (sb.Length == 3)
                    {
                        return sb.ToString();
                    }
                }
            }

            // add padding X
            while (sb.Length < 3)
            {
                sb.Append("X");
            }

            return sb.ToString();
        }

        private static bool IsVowel(char c)
        {
            var vowels = new char[] { 'A', 'E', 'I', 'O', 'U' };
            return vowels.Contains(c);
        }
    }
}

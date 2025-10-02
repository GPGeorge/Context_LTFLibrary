# Master Lists - Corrected Implementation Checklist

## ✅ Corrections Applied

All files have been updated to match your actual database schema:

### Table Names
- ✅ `tblParticipant` (singular, not plural)
- ✅ `tblPublicationTransfer` (not `tblPublicationParticipant`)

### Column Names
- ✅ Database uses: `Genre`, `MediaType`, `MediaCondition`, `ParticipantStatus` (no "1")
- ✅ C# models use: `Genre1`, `MediaType1`, `MediaCondition1`, `ParticipantStatus1` (with "1")
- ✅ SQL scripts updated to use database names
- ✅ C# code uses model property names (already correct)

---

## 🎯 Step 1: Verify Database (Do This Now)

Run the updated **Database Verification Script**:

```
✓ Updated with correct table name: tblParticipant
✓ Updated with correct junction table: tblPublicationTransfer
✓ Updated to show database column names (without "1")
```

**What to check:**
- [ ] All 9 tables exist
- [ ] Column names shown match expectations
- [ ] Check for any duplicate values
- [ ] Note any existing indexes
- [ ] Verify junction tables exist

---

## 🎯 Step 2: Create Indexes (After Verification)

Run the updated **Database Indexes Script**:

```
✓ Uses correct table name: tblParticipant
✓ Uses database column names (without "1")
✓ Creates unique indexes to prevent duplicates
✓ Creates performance indexes for sorting/filtering
```

**Safe to run:**
- ✅ Creates indexes only if they don't already exist
- ✅ Won't affect existing data
- ✅ Will improve performance and data integrity

---

## 🎯 Step 3: Note Any Issues Found

### If Duplicates Are Found:

The verification script will show you any duplicate values. You'll need to decide:

**Option A: Keep First Occurrence**
```sql
-- Example for Genre duplicates
WITH DuplicateGenres AS (
    SELECT GenreID, Genre,
           ROW_NUMBER() OVER (PARTITION BY Genre ORDER BY GenreID) AS RowNum
    FROM tblGenre
)
-- Review duplicates first
SELECT * FROM DuplicateGenres WHERE RowNum > 1;

-- After review, you could delete duplicates (BE CAREFUL!)
-- DELETE FROM tblGenre WHERE GenreID IN (
--     SELECT GenreID FROM DuplicateGenres WHERE RowNum > 1
-- );
```

**Option B: Rename Duplicates**
```sql
-- Add a suffix to make them unique
UPDATE tblGenre 
SET Genre = Genre + ' (duplicate)'
WHERE GenreID IN (SELECT GenreID FROM DuplicatesListHere);
```

### If Tables Are Missing:

Contact me and we'll adjust the implementation to work without those tables.

---

## 🎯 Step 4: Verify Your C# Models

Before implementing the C# code, verify your Entity Framework models have these properties:

### Check Data.Models.Genre class:
```csharp
public class Genre
{
    public int GenreID { get; set; }
    public string Genre1 { get; set; }  // Should have "1" suffix
    public int? SortOrder { get; set; }
}
```

### Check Data.Models.MediaType class:
```csharp
public class MediaType
{
    public int MediaTypeID { get; set; }
    public string MediaType1 { get; set; }  // Should have "1" suffix
    public int? SortOrder { get; set; }
}
```

### Check Data.Models.MediaCondition class:
```csharp
public class MediaCondition
{
    public int MediaConditionID { get; set; }
    public string MediaCondition1 { get; set; }  // Should have "1" suffix
    public int? SortOrder { get; set; }
}
```

### Check Data.Models.ParticipantStatus class:
```csharp
public class ParticipantStatus
{
    public int ParticipantStatusID { get; set; }
    public string ParticipantStatus1 { get; set; }  // Should have "1" suffix
    public int? SortOrder { get; set; }
}
```

### Check Data.Models.Participant class (singular):
```csharp
public class Participant  // Should be singular
{
    public int ParticipantID { get; set; }
    public string ParticipantFirstName { get; set; }
    public string ParticipantMiddleName { get; set; }
    public string ParticipantLastName { get; set; }
}
```

### Check Data.Models.PublicationTransfer class:
```csharp
public class PublicationTransfer
{
    public int PublicationID { get; set; }
    public int ParticipantID { get; set; }
    public int? ParticipantStatusID { get; set; }
    // ... other fields
}
```

**If your model property names are different**, let me know and I'll adjust the service code.

---

## 📋 Database Verification Results Template

After running the verification script, record your findings:

```
Database Name: ________________

✓ Tables Exist:
  [ ] tblBookcase (______ rows)
  [ ] tblCreator (______ rows)
  [ ] tblGenre (______ rows)
  [ ] tblPublisher (______ rows)
  [ ] tblMediaType (______ rows)
  [ ] tblMediaCondition (______ rows)
  [ ] tblParticipant (______ rows)
  [ ] tblParticipantStatus (______ rows)
  [ ] tblShelf (______ rows)
  [ ] tblPublication (______ rows)

✓ Junction Tables:
  [ ] tblPublicationCreator
  [ ] tblPublicationGenre
  [ ] tblPublicationTransfer

✓ Column Names Verified:
  [ ] tblGenre has 'Genre' (not 'Genre1')
  [ ] tblMediaType has 'MediaType' (not 'MediaType1')
  [ ] tblMediaCondition has 'MediaCondition' (not 'MediaCondition1')
  [ ] tblParticipantStatus has 'ParticipantStatus' (not 'ParticipantStatus1')

✓ Duplicates Check:
  [ ] No duplicates found (ready for unique indexes)
  [ ] Duplicates found in: ________________ (need to resolve)

✓ Existing Indexes:
  [ ] Listed indexes: ________________
```

---

## ⚠️ Important Notes

### Do NOT implement C# code until:
1. ✅ Database verification is complete
2. ✅ Any duplicates are resolved
3. ✅ Unique indexes are created
4. ✅ C# model properties are verified

### The "1" Suffix Situation:
- **In Database**: Column names have NO "1" suffix ✓
- **In C# Models**: Property names have "1" suffix ✓
- **This is normal and correct** - Entity Framework's automatic naming

### PowerApps Compatibility:
- **These changes won't affect your PowerApps** - we're only adding indexes
- **Indexes are transparent** to applications
- **PowerApps will continue working** exactly as before

---

## 🚀 Ready to Proceed?

Once you've completed the database verification:

1. **Share your results** - especially:
   - Any duplicates found
   - Any unexpected column names
   - Any missing tables

2. **I'll confirm** the C# code is correct for your schema

3. **Then you can proceed** with confidence to implement the C# files

---

## 📞 Questions to Answer

Before proceeding with C# implementation, please confirm:

- [ ] Ran verification script successfully
- [ ] All 9 lookup tables exist
- [ ] All 3 junction tables exist  
- [ ] Column names match (Genre not Genre1 in database)
- [ ] No duplicate values (or duplicates resolved)
- [ ] Indexes created successfully
- [ ] Ready to proceed with C# code

Take your time with the database verification - getting this foundation right will make everything else smooth!

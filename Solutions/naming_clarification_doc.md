# Database vs C# Model Naming - Important Clarification

## 🎯 The Key Issue: Entity Framework Naming Quirk

Your database has **clean column names**, but Entity Framework added "1" suffixes to avoid naming conflicts in the C# models.

---

## 📊 Naming Comparison Table

| What | Database Column Name | C# Model Property Name | Why Different? |
|------|---------------------|----------------------|----------------|
| Genre | `Genre` | `Genre1` | EF avoids table/column name conflicts |
| Media Type | `MediaType` | `MediaType1` | EF avoids table/column name conflicts |
| Media Condition | `MediaCondition` | `MediaCondition1` | EF avoids table/column name conflicts |
| Participant Status | `ParticipantStatus` | `ParticipantStatus1` | EF avoids table/column name conflicts |
| Participant Table | `tblParticipant` | `Participant` | Table is singular |
| Junction Table | `tblPublicationTransfer` | `PublicationTransfer` | Your specific name |

---

## 🔧 What This Means For Implementation

### ✅ In SQL Scripts (Use Database Names - NO "1" suffix)
```sql
-- Correct
SELECT Genre FROM tblGenre
SELECT MediaType FROM tblMediaType
SELECT MediaCondition FROM tblMediaCondition
SELECT ParticipantStatus FROM tblParticipantStatus

-- Wrong - these columns don't exist
SELECT Genre1 FROM tblGenre  -- ❌
SELECT MediaType1 FROM tblMediaType  -- ❌
```

### ✅ In C# LINQ Code (Use Model Property Names - WITH "1" suffix)
```csharp
// Correct - using EF model properties
var genre = new Data.Models.Genre
{
    Genre1 = item.Name,  // ✓ Correct
    SortOrder = item.SortOrder
};

// Wrong - these properties don't exist in C# models
var genre = new Data.Models.Genre
{
    Genre = item.Name,  // ❌ Won't compile
    SortOrder = item.SortOrder
};
```

---

## 📝 Corrections Made

### 1. Table Names Fixed
- ❌ Old: `tblParticipants` (plural)
- ✅ New: `tblParticipant` (singular)

### 2. Junction Table Name Fixed
- ❌ Old: `tblPublicationParticipant`
- ✅ New: `tblPublicationTransfer`

### 3. SQL Scripts Updated
All SQL scripts now use correct database column names (without "1" suffix):
- `Genre` not `Genre1`
- `MediaType` not `MediaType1`
- `MediaCondition` not `MediaCondition1`
- `ParticipantStatus` not `ParticipantStatus1`

### 4. C# Service Code
**The C# code was already correct!** It uses the EF model property names (with "1" suffix):
- `Genre1` in C# code ✓
- `MediaType1` in C# code ✓
- `MediaCondition1` in C# code ✓
- `ParticipantStatus1` in C# code ✓

This is because we're querying the EF models, not the database directly.

---

## 🚀 Files Ready to Use

All files have been corrected and are ready for implementation:

### ✅ SQL Scripts (Corrected)
1. **Database Verification Script** - Uses correct table/column names
2. **Database Indexes Script** - Uses correct table/column names

### ✅ C# Files (Already Correct)
3. **IMasterListService.cs** - No changes needed
4. **MasterListDtos.cs** - No changes needed
5. **MasterListService.cs** - Updated to use `tblPublicationTransfer`
6. **MasterLists.razor** - No changes needed

---

## 🎯 Next Steps

You can now proceed with confidence:

1. **Run the updated verification script** to confirm everything matches
2. **Run the indexes script** to add unique indexes
3. **Implement the C# files** - they're ready as-is

---

## 💡 Why Does EF Do This?

Entity Framework automatically renames properties when it scaffolds a database to avoid this situation:

```csharp
// If EF didn't rename, you'd have:
public class Genre
{
    public int GenreID { get; set; }
    public string Genre { get; set; }  // Same name as class!
}

// So EF renames to:
public class Genre
{
    public int GenreID { get; set; }
    public string Genre1 { get; set; }  // Numbered to avoid conflict
}
```

This is a well-known EF quirk, and it's actually helping avoid naming conflicts in the C# code. Your database design is clean - it's just the C# models that have the adjusted names.

---

## 📊 Summary

| Layer | Use Names... |
|-------|-------------|
| SQL Scripts | WITHOUT "1" (Genre, MediaType, etc.) |
| C# LINQ Code | WITH "1" (Genre1, MediaType1, etc.) |
| Your Database | Clean names ✓ |
| Your C# Models | EF-adjusted names ✓ |

**Both are correct for their respective layers!**

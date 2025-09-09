# Email to Paul - SQL Developer

**To:** Paul [SQL Developer]  
**From:** [Your Name]  
**Subject:** JSON Query Request - As Discussed Yesterday  
**Date:** September 5, 2025

---

Hi Paul,

Thanks for taking the time to chat yesterday! I really appreciate you offering to help with this before you head off on holiday Monday.

As we discussed, I need a SQL query that returns patient and lab data in a specific JSON format. The data will feed into our new integration service that handles the message processing side of things.

## What I Need

Could you create a SQL query that returns patient data in the JSON structure I've attached? I've included some sample data so you can see exactly what format we need.

The JSON should include these main data sections:

- **Patient Information** - Basic demographics, IDs, and address details
- **Visit Details** - Hospital/facility info and attending doctor
- **Lab Orders** - Test requests and order information  
- **Results Data** - Lab results and reports

I know this matches up well with what we have in the `Molec_WCP_Uploader` view we looked at.

## Database Context

Based on our `Molec_WCP_Uploader` view, the key fields we need are:

- **Patient Data:** `p.SOCIAL_SECURITY`, `p.FIRSTNAME`, `p.LASTNAME`, `p.DOB`, `p.SEX`, `p.PHYSICIAN`
- **Location Data:** `di.DESCRIPTION` (district/facility)
- **Message Data:** `hl7.MESSAGE_CONTENT`, `hl7.CREATION_DATE`
- **Identifiers:** `p.INTID`, `hl7.PATIENT_ID`

## Requirements

1. **SQL query** using SQL Server's `FOR JSON` functionality
2. **Sample/dummy data** for testing (realistic but fictitious values)
3. **Field mappings** showing how our SHIRE database fields map to the JSON structure
4. **Handle multiple patients** - return as an array of JSON objects

## Technical Notes

- Standard SQL Server setup (same as our existing SHIRE environment)
- Use `FOR JSON` to return the structure
- Include any necessary JOINs for complete patient and message data
- Consider using `ROW_NUMBER()` if needed to handle duplicates (like the current view)

## Example Field Mappings

Here's how I'm thinking the database fields map to the JSON:

- Patient Name → `p.LASTNAME`, `p.FIRSTNAME`
- Date of Birth → `p.DOB` (formatted as YYYYMMDD)
- Gender → `p.SEX`
- NHS Number → `p.SOCIAL_SECURITY`
- Attending Doctor → `p.PHYSICIAN`
- Facility → `di.DESCRIPTION`

## Timeline

Perfect timing with you getting this done before Monday! No rush though - whenever you get a chance over the next couple of days would be great.

Let me know if you need any clarification on the JSON structure - it's all in the attached file. And have a fantastic holiday!

Cheers,  
[Your Name]

---

## Attachments

1. **`hl7-message-parsed.json`** - The exact JSON structure we need, with sample data
2. **`hl7-database-schema.json`** - Database field mappings and schema details

## Reference

- **`Molec_WCP_Uploader` view** - The SHIRE database view we discussed yesterday

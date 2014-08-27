FluentMigrator DB2 support notes

This code was designed to work with DB2 for the AS/400 system.  
All classes have been tested with the v5r4 release of DB2 for AS/400, using the IBM DB2 for i5/OS .NET Provider. 
Attempts were made to provide basic support for other versions of DB2, but no other versions have been tested.

Notes:

**Handling of drop/alter statements on an AS/400 system**
Certain alter or drop statements, when used on an AS/400 system, will require a confirmation if the action is expected to incur data loss.
As it is not possible to reply to this message through the iSeries provider, there must be an automated repsonse to the inquiry. In testing, 
the provider made no attempts to reply to the message, causing the program to wait for a reply. At which point, no other queries could be 
executed until a manual reply was made. If you encounter problems with certain drop or alter statements, it may be necessary to create a 
reply list entry with message id CPD32CC.
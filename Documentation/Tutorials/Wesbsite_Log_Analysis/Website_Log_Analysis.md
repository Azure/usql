
# Website Log Analysis


## Introduction

This tutorial demonstrates how to analyze website logs using Azure Data Lake Analytics.


## Step 1: Sample Data

If you haven’t done so:

* Navigate to your ADLA account in the Azure Portal
* Under **Getting Started** click **Sample Scripts**. If it appears click on the notification for "Sample Data"

This Tutorial will use following input file

    /Samples/Data/WebLog.log

Navigate to this sample file and ensure you can view it

## Step 2: Create the weblogs TVF

Because you will use the data in multiple Data Lake Analytics jobs, it will be easier to store the data in a Table Valued Function (TVF). 
The TVF ensures that your jobs fetch data from the weblog file with the correct schema. This also makes it easy for other people to reuse the data.

Review the U-SQL script, and then click Submit. It takes a few minutes for the job to complete. 

    CREATE DATABASE IF NOT EXISTS SampleDBTutorials;

    DROP FUNCTION IF EXISTS SampleDBTutorials.dbo.WeblogsView;

    //create TVF WeblogsView on space-delimited website log data
    CREATE FUNCTION SampleDBTutorials.dbo.WeblogsView()
    RETURNS @result TABLE
    (
        s_date DateTime,
        s_time string,
        s_sitename string,
        cs_method string, 
        cs_uristem string,
        cs_uriquery string,
        s_port int,
        cs_username string, 
        c_ip string,
        cs_useragent string,
        cs_cookie string,
        cs_referer string, 
        cs_host string,
        sc_status int,
        sc_substatus int,
        sc_win32status int, 
        sc_bytes int,
        cs_bytes int,
        s_timetaken int
    )
    AS
    BEGIN

        @result = EXTRACT
            s_date DateTime,
            s_time string,
            s_sitename string,
            cs_method string,
            cs_uristem string,
            cs_uriquery string,
            s_port int,
            cs_username string,
            c_ip string,
            cs_useragent string,
            cs_cookie string,
            cs_referer string,
            cs_host string,
            sc_status int,
            sc_substatus int,
            sc_win32status int,
            sc_bytes int,
            cs_bytes int,
            s_timetaken int
        FROM @"/Samples/Data/WebLog.log"
        USING Extractors.Text(delimiter:' ');

    END;

Wait for this job to complete.


## Step 3: Analyze Referrers (Incoming Links)

In this step, you will find out how often someone experiences successes or failures by creating a table called ReferrersPerDay that groups the referrers and HTTP statuses. You will output the results into a table so future analysis are more performant.

Review the U-SQL script, and then click Submit. It takes a few minutes for the job to complete. 

    DROP TABLE IF EXISTS SampleDBTutorials.dbo.ReferrersPerDay;

    //create table ReferrersPerDay for storing references from external websites
    CREATE TABLE SampleDBTutorials.dbo.ReferrersPerDay
    (
        INDEX idx1
        CLUSTERED(Year ASC)
        PARTITIONED BY HASH(Year)
    ) AS 

    SELECT s_date.Year AS Year,
        s_date.Month AS Month,
        s_date.Day AS Day,
        cs_referer,
        sc_status,
        COUNT(DISTINCT c_ip) AS cnt
    FROM SampleDBTutorials.dbo.WeblogsView() AS weblog
    GROUP BY s_date,
            cs_referer, 
            sc_status;

## Step 4: Identify Unsuccessful Requests

In this step, you will find out which of the referrers run into errors when they try to visit the website.

Review the U-SQL script, and then click Submit. It takes a few minutes for the job to complete. 

    @content =
        SELECT *
        FROM SampleDBTutorials.dbo.ReferrersPerDay
        WHERE sc_status >=400 AND sc_status < 500;

    OUTPUT @content
        TO @"/Samples/Outputs/UnsuccessfulResponses.log"
        USING Outputters.Tsv();


## You are Done

At this point you've submitted 3 U-SQL Jobs to analyze a website log. The outputs were text file and a U-SQL table.


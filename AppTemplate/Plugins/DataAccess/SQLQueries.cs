//
// The MIT License (MIT)
//
// Copyright (C) 2012 Gary McNickle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


namespace DataAccess
{
   internal static class SQLQueries
   {
      internal static string SHIPS_BY_RACE_AND_CLASS = @"SELECT
                                                         t.*, 
                                                         g.groupName,
                                                         r.raceName,
                                                         m.marketGroupName marketGroupName,
                                                         mp.marketGroupName marketParentGroupName
                                                      FROM
                                                        chrRaces AS r INNER JOIN
                                                        invTypes AS t ON r.raceID = t.raceID INNER JOIN
                                                        invGroups AS g ON t.groupID = g.groupID LEFT OUTER JOIN
                                                        invMarketGroups AS m ON t.marketGroupID = m.marketGroupID LEFT JOIN
                                                        invMarketGroups AS mp ON m.parentGroupID = mp.marketGroupID 
                                                      WHERE
                                                        g.categoryID = 6 AND t.published = 1
                                                      ORDER BY
                                                        t.typeName ASC";

      internal static string ALL_PUBLISHED_MODULES = @"SELECT 
                                                      invTypes.typeID, invTypes.typeName, invTypes.description
                                                      FROM invTypes
                                                      LEFT JOIN invGroups ON invTypes.groupID = invGroups.groupID
                                                      WHERE invTypes.published = 1 AND invGroups.categoryID = 7";

      internal static string BASIC_SHIP_FITTING = @"SELECT
                                                   dgmtypeattributes.typeID,
                                                   invtypes.typeName,
                                                   dgmattributetypes.attributeName,
                                                   dgmtypeattributes.attributeID,
                                                   dgmtypeattributes.valueInt,
                                                   dgmtypeattributes.valueFloat,
                                                   invtypes.groupID,
                                                   invgroups.groupName,
                                                   invtypes.raceID,
                                                   chrraces.raceName
                                                   FROM
                                                   dgmtypeattributes
                                                   INNER JOIN dgmattributetypes ON dgmattributetypes.attributeID = dgmtypeattributes.attributeID
                                                   INNER JOIN invtypes ON dgmtypeattributes.typeID = invtypes.typeID
                                                   INNER JOIN invgroups ON invtypes.groupID = invgroups.groupID
                                                   INNER JOIN chrraces ON invtypes.raceID = chrraces.raceID
                                                   {0} 
                                                   AND dgmtypeattributes.attributeID IN (12,13,14,283,1137)";
   }
}

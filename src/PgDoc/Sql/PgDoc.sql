-- Copyright 2016 Flavien Charlon
--
-- Licensed under the Apache License, Version 2.0 (the "License");
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
--
--     http://www.apache.org/licenses/LICENSE-2.0
--
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.

-- ======================================================================
-- get_document_type: Get the type of an entity.
-- ======================================================================

CREATE OR REPLACE FUNCTION get_document_type(id uuid)
    RETURNS int
AS $$ DECLARE
    bytes bytea;
BEGIN
    bytes = decode(substring(id::text, 1, 8), 'hex');
    RETURN (get_byte(bytes, 0)::int << 24)
        | (get_byte(bytes, 1)::int << 16)
        | (get_byte(bytes, 2)::int << 8)
        | (get_byte(bytes, 3)::int);
END $$ LANGUAGE plpgsql
IMMUTABLE;

-- ======================================================================
-- document_of_type: A virtual table of all the documents with a certain
-- document type.
-- ======================================================================

CREATE OR REPLACE FUNCTION document_of_type(document_type int)
RETURNS SETOF document AS $$
    SELECT id, body, version
    FROM document
    WHERE 
        body IS NOT NULL
        AND get_document_type(id) = document_type;
$$ LANGUAGE SQL
STABLE;

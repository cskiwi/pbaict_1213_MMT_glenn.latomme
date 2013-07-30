use remp;

# test selections

-- SELECT * FROM artists_has_tags WHERE Artist_id = 1;
-- INSERT INTO Songs (Artist_id, Name) VALUES('52','Take me away');
-- SELECT COUNT(*) From Songs WHERE Artist_id = '1';
-- INSERT INTO songs (Artist_id, Name)VALUES('1',  'test');
-- DELETE FROM artists_has_tags WHERE Artist_id = 41;
-- DELETE FROM tags WHERE Tag = 'Hip-Hop';

-- update Artists set Name = lower(Name) where id < 9999999;

-- SELECT * FROM Artists 
-- INNER JOIN artists_has_tags on artists_has_tags.Artist_id = Artists.id
-- WHERE artists_has_tags.tag = 'dnb';

-- SELECT COUNT(*) FROM artists_has_tags WHERE Tag = 'dnb';

-- SELECT tag FROM artists_has_tags
-- INNER JOIN Artists on Artists.id = artists_has_tags.Artist_id
-- WHERE Artists.Name = 'Spor'; # spaces in name can crash the damn thing

# addings

-- INSERT INTO Artists (Name)VALUES('Camo & Krooked');

-- INSERT INTO tags (Tag)VALUES('Drum and Bass');
-- INSERT INTO tags (Tag)VALUES('Liquid');
-- INSERT INTO tags (Tag)VALUES('Liquid Funk');
-- INSERT INTO tags (Tag)VALUES('Neuro Funk');
-- INSERT INTO tags (Tag)VALUES('Electronic');
-- INSERT INTO tags (Tag)VALUES('Dnb');
-- 
-- INSERT INTO artists_has_tags (Artist_id, Tag)VALUES(1,  'Drum and Bass');
-- INSERT INTO artists_has_tags (Artist_id, Tag)VALUES(1,  'Liquid Funk');
-- INSERT INTO artists_has_tags (Artist_id, Tag)VALUES(1,  'Neuro Funk');
-- INSERT INTO artists_has_tags (Artist_id, Tag)VALUES(1,  'Neuro');

-- INSERT INTO Songs (Artist_id, Name) VALUES('133','Cobra');
-- SELECT COUNT(*) From `Songs` WHERE `Artist_id` = '122' AND Name = 'burnin\\ and lootin\\';

-- SELECT Songs.Name FROM Songs INNER JOIN Artists on Artists.id = Songs.Artist_id WHERE Artists.Name = 'Spor';
-- SELECT id FROM artists WHERE Name = 'Aztec';

# output all tables

SELECT * From Artists;
SELECT * From Songs;
SELECT * FROM tags;
SELECT * FROM artists_has_tags;

-- SELECT TABLE_NAME, TABLE_ROWS 
-- FROM `information_schema`.`tables` 
-- WHERE `table_schema` = 'remp';
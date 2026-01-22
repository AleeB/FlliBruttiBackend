-- MySQL dump 10.13  Distrib 8.4.7, for macos15 (arm64)
--
-- Host: 127.0.0.1    Database: flliBrutti
-- ------------------------------------------------------
-- Server version	8.4.7

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `firme`
--

DROP TABLE IF EXISTS `firme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `firme` (
  `idfirme` bigint NOT NULL AUTO_INCREMENT,
  `entrata` datetime DEFAULT NULL,
  `uscita` datetime DEFAULT NULL,
  `idUser` bigint DEFAULT NULL,
  PRIMARY KEY (`idfirme`),
  KEY `idUser_idx` (`idUser`),
  CONSTRAINT `idUser` FOREIGN KEY (`idUser`) REFERENCES `users` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `firme`
--

LOCK TABLES `firme` WRITE;
/*!40000 ALTER TABLE `firme` DISABLE KEYS */;
INSERT INTO `firme` VALUES (1,'2026-01-06 19:08:51','2026-01-07 18:55:03',1),(2,'2026-01-14 16:10:21','2026-01-14 16:10:27',2),(3,'2026-01-14 21:58:40',NULL,1),(4,'2026-01-14 22:04:56',NULL,6);
/*!40000 ALTER TABLE `firme` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `formulapreventivo`
--

DROP TABLE IF EXISTS `formulapreventivo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `formulapreventivo` (
  `costo_km` float NOT NULL,
  `primo_autista` float NOT NULL,
  `seconto_autista` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `formulapreventivo`
--

LOCK TABLES `formulapreventivo` WRITE;
/*!40000 ALTER TABLE `formulapreventivo` DISABLE KEYS */;
/*!40000 ALTER TABLE `formulapreventivo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `people`
--

DROP TABLE IF EXISTS `people`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `people` (
  `idPerson` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  `surname` varchar(50) DEFAULT NULL,
  `phoneNumber` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`idPerson`),
  UNIQUE KEY `phoneNumber_UNIQUE` (`phoneNumber`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `people`
--

LOCK TABLES `people` WRITE;
/*!40000 ALTER TABLE `people` DISABLE KEYS */;
INSERT INTO `people` VALUES (1,'Gianni','Brutti','1970-01-07'),(2,'Alessio','Brutti','2002-05-25'),(6,'1','1','2026-01-14'),(7,'2','2','2026-01-16');
/*!40000 ALTER TABLE `people` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `preventiviextra`
--

DROP TABLE IF EXISTS `preventiviextra`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `preventiviextra` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `costo` double NOT NULL,
  `description` mediumtext NOT NULL,
  `idPreventivo` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idPreventivoRef_idx` (`idPreventivo`),
  CONSTRAINT `idPreventivoRef` FOREIGN KEY (`idPreventivo`) REFERENCES `preventivincc` (`idPreventivo`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `preventiviextra`
--

LOCK TABLES `preventiviextra` WRITE;
/*!40000 ALTER TABLE `preventiviextra` DISABLE KEYS */;
INSERT INTO `preventiviextra` VALUES (1,142.2,'ztl milano',1),(2,1000,'ztl saint moritz',1);
/*!40000 ALTER TABLE `preventiviextra` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `preventivincc`
--

DROP TABLE IF EXISTS `preventivincc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `preventivincc` (
  `idPreventivo` bigint NOT NULL AUTO_INCREMENT,
  `description` longtext NOT NULL,
  `costo` double DEFAULT NULL,
  `is_todo` tinyint DEFAULT '1',
  `partenza` varchar(100) DEFAULT NULL,
  `arrivo` varchar(100) DEFAULT NULL,
  `idUser` bigint DEFAULT NULL,
  `idUserNonAutenticato` bigint DEFAULT NULL,
  PRIMARY KEY (`idPreventivo`),
  KEY `idUser_idx` (`idUser`),
  KEY `idUserNonAutenticato_idx` (`idUserNonAutenticato`),
  CONSTRAINT `idUserNonAutenticatoRef` FOREIGN KEY (`idUserNonAutenticato`) REFERENCES `usersnotauthenticated` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `idUserRef` FOREIGN KEY (`idUser`) REFERENCES `users` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `preventivincc`
--

LOCK TABLES `preventivincc` WRITE;
/*!40000 ALTER TABLE `preventivincc` DISABLE KEYS */;
INSERT INTO `preventivincc` VALUES (1,'nigga',2314,0,'gay','finocchio',1,2);
/*!40000 ALTER TABLE `preventivincc` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refreshtokens`
--

DROP TABLE IF EXISTS `refreshtokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refreshtokens` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `token` varchar(200) NOT NULL,
  `userId` bigint NOT NULL,
  `expiresAt` datetime NOT NULL,
  `createdAt` datetime NOT NULL,
  `isRevoked` tinyint NOT NULL DEFAULT '0',
  `revokedByIp` varchar(50) DEFAULT NULL,
  `revokedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `token_idx` (`token`),
  KEY `userId_idx` (`userId`),
  CONSTRAINT `userId` FOREIGN KEY (`userId`) REFERENCES `users` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=183 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refreshtokens`
--

LOCK TABLES `refreshtokens` WRITE;
/*!40000 ALTER TABLE `refreshtokens` DISABLE KEYS */;
INSERT INTO `refreshtokens` VALUES (182,'nt3dKK6L/BgonBo6RKJHnjySGeeMLxA+Z/LZiOvfEKUnZExTd1X6KpeaYBO+ARJ+n3DI6L7uBicJK1dvUQTqEA==',6,'2026-01-31 21:33:02','2026-01-16 21:33:02',0,NULL,NULL);
/*!40000 ALTER TABLE `refreshtokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `idPerson` bigint NOT NULL,
  `type` int NOT NULL,
  `email` varchar(100) NOT NULL,
  `password` varchar(100) NOT NULL,
  PRIMARY KEY (`email`),
  UNIQUE KEY `idPerson_UNIQUE` (`idPerson`),
  CONSTRAINT `idPersonRef` FOREIGN KEY (`idPerson`) REFERENCES `people` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (6,1,'1','Y0N9xNCMilgY8PTcfPqRfEKKhQT1DEMLfbDAYBoa65y+js9ZM0rnFdh//Bj58hGU'),(7,2,'2','56zAm2vU+gDMV6v9X1mlGlG5clo6DkB1ku7XWR0YOzuLORSdK9RIq72BRFiEiVRi'),(2,1,'alessiobrutti@outlook.com','/JMYDQ8PAvl2dJe3ZH8SyFD60OUq84fkdU5ELqqh81aDa+0F174lRi4S8jlEnHVl'),(1,1,'giannibrutti@virgilio.it','CZz+DSChHp6aKllf7ioeifmIIggRpwyt+PVDRWjPpuCOrT5S/MP3Wn5Jhh0Q6wII');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usersnotauthenticated`
--

DROP TABLE IF EXISTS `usersnotauthenticated`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usersnotauthenticated` (
  `idPerson` bigint NOT NULL,
  `ip` varchar(20) NOT NULL,
  `email` varchar(45) NOT NULL,
  PRIMARY KEY (`email`),
  UNIQUE KEY `ipPerson_UNIQUE` (`idPerson`),
  CONSTRAINT `idPersonNotAuthenticatedRef` FOREIGN KEY (`idPerson`) REFERENCES `people` (`idPerson`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usersnotauthenticated`
--

LOCK TABLES `usersnotauthenticated` WRITE;
/*!40000 ALTER TABLE `usersnotauthenticated` DISABLE KEYS */;
INSERT INTO `usersnotauthenticated` VALUES (2,'192.168.1.1','alessiobrutti@outlook.com');
/*!40000 ALTER TABLE `usersnotauthenticated` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-22 13:45:25

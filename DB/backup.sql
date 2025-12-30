-- MySQL dump 10.13  Distrib 8.4.7, for macos15 (arm64)
--
-- Host: localhost    Database: flliBrutti
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `firme`
--

LOCK TABLES `firme` WRITE;
/*!40000 ALTER TABLE `firme` DISABLE KEYS */;
/*!40000 ALTER TABLE `firme` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `formulaPreventivo`
--

DROP TABLE IF EXISTS `formulaPreventivo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `formulaPreventivo` (
  `costo_km` float NOT NULL,
  `primo_autista` float NOT NULL,
  `seconto_autista` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `formulaPreventivo`
--

LOCK TABLES `formulaPreventivo` WRITE;
/*!40000 ALTER TABLE `formulaPreventivo` DISABLE KEYS */;
/*!40000 ALTER TABLE `formulaPreventivo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `persons`
--

DROP TABLE IF EXISTS `persons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `persons` (
  `idperson` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  `surname` varchar(50) DEFAULT NULL,
  `dob` date DEFAULT NULL,
  PRIMARY KEY (`idperson`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `persons`
--

LOCK TABLES `persons` WRITE;
/*!40000 ALTER TABLE `persons` DISABLE KEYS */;
/*!40000 ALTER TABLE `persons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `preventivi`
--

DROP TABLE IF EXISTS `preventivi`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `preventivi` (
  `idPreventivi` bigint NOT NULL AUTO_INCREMENT,
  `description` longtext NOT NULL,
  `costo` double DEFAULT NULL,
  `is_todo` tinyint DEFAULT '1',
  `idUser` bigint DEFAULT NULL,
  `idUserNonAutenticato` bigint DEFAULT NULL,
  PRIMARY KEY (`idPreventivi`),
  KEY `idUser_idx` (`idUser`),
  KEY `idUserNonAutenticato_idx` (`idUserNonAutenticato`),
  CONSTRAINT `idUserNonAutenticatoRef` FOREIGN KEY (`idUserNonAutenticato`) REFERENCES `usersNotAuthenticated` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `idUserRef` FOREIGN KEY (`idUser`) REFERENCES `users` (`idPerson`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `preventivi`
--

LOCK TABLES `preventivi` WRITE;
/*!40000 ALTER TABLE `preventivi` DISABLE KEYS */;
/*!40000 ALTER TABLE `preventivi` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `idPerson` bigint NOT NULL,
  `type` varchar(15) NOT NULL,
  `email` varchar(100) NOT NULL,
  `password` varchar(100) DEFAULT NULL,
  UNIQUE KEY `idPerson_UNIQUE` (`idPerson`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usersNotAuthenticated`
--

DROP TABLE IF EXISTS `usersNotAuthenticated`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usersNotAuthenticated` (
  `idPerson` bigint NOT NULL,
  `ip` varchar(20) NOT NULL,
  UNIQUE KEY `ipPerson_UNIQUE` (`idPerson`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usersNotAuthenticated`
--

LOCK TABLES `usersNotAuthenticated` WRITE;
/*!40000 ALTER TABLE `usersNotAuthenticated` DISABLE KEYS */;
/*!40000 ALTER TABLE `usersNotAuthenticated` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-30 15:16:39

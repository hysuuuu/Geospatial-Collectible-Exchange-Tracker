package main

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/jackc/pgx/v5"
	"github.com/joho/godotenv"
)

const (
	CenterLat     = 33.6435
	CenterLng     = -117.8445
	Offset        = 0.05
	COUNT         = 1
	SpawnInterval = 10
)

func main() {
	_ = godotenv.Load(".env")

	connString := os.Getenv("DATABASE_URL")
	if connString == "" {
		log.Fatal("DATABASE_URL is not set")
	}

	conn, err := pgx.Connect(context.Background(), connString)
	if err != nil {
		log.Fatalf("DB connection failed: %v", err)
	}

	defer conn.Close(context.Background())

	fmt.Println("Db connection success. Go worker running...")

	// Generate every 10 minutes
	ticker := time.NewTicker(SpawnInterval * time.Minute)
	defer ticker.Stop()

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)

	// spawnCollectibles(conn, COUNT)
	fmt.Println("Waiting for next spawn.. (Ctrl + c to stop)")

	for {
		select {
		case <-ticker.C:
			fmt.Printf("\n[%s] Generating new item...\n", time.Now().Format("15:04:05"))
			spawnCollectibles(conn, COUNT)
		case <-quit:
			fmt.Println("\n Wroker terminated.")
			return
		}
	}
}

func spawnCollectibles(conn *pgx.Conn, count int) {
	rand.NewSource((time.Now().UnixNano()))

	sql := `
		INSERT INTO "Collectibles" ("Name", "Location", "CreatedAt") 
		VALUES ($1, ST_SetSRID(ST_MakePoint($2, $3), 4326), $4)
	`
	insertCount := 0

	for i := 0; i < count; i++ {
		lat := CenterLat + (rand.Float64()-0.5)*Offset
		lng := CenterLng + (rand.Float64()-0.5)*Offset
		itemName := fmt.Sprintf("Item #%d", rand.Intn(9999))
		createdAt := time.Now()

		_, err := conn.Exec(context.Background(), sql, itemName, lng, lat, createdAt)

		if err != nil {
			log.Printf("Insert item %d failed: %v\n", i, err)
		} else {
			insertCount++
		}
	}
}

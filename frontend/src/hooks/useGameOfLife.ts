import { useEffect, useRef, useState } from "react";
import {
    getNextBoard,
    createBoard as createNewBoard,
    advanceNSteps,
    start as startGame,
    stop as stopGame
} from "../api/gameOfLifeApi";
import type { HubConnection } from "@microsoft/signalr";
import { createSignalRConnection } from "../services/signalRConnection";

export function useGameOfLife(rows: number, cols: number) {
    const [grid, setGrid] = useState<number[][]>(
        Array.from({ length: rows }, () => Array(cols).fill(0))
    );
    const [isRunning, setIsRunning] = useState(false);
    const [generation, setGeneration] = useState(0);
    const [boardId, setBoardId] = useState<string>("");
    const connectionRef = useRef<HubConnection | null>(null);

    useEffect(() => {
        if (!boardId) return;

        const connection = createSignalRConnection();
        connectionRef.current = connection;

        async function startConnection() {
            try {
                await connection.start();
                console.log("✅ Connected to SignalR hub");

                // Subscribe to the board
                await connection.send("StartBoard", boardId);

                // Listen for updates
                connection.on("UpdateBoard", (data) => {
                    console.log("📡 Update received:", data);
                    setGrid(data.grid);
                    setGeneration(data.generation);
                });
            } catch (err) {
                console.error("❌ SignalR connection error:", err);
            }
        }

        startConnection();

        return () => {
            connection.stop();
        };
    }, [boardId]);

    async function advanceSteps(steps: number) {
        if (steps <= 0) return;
        try {
            let id = boardId;
            if (!id) {
                id = await createBoard() ?? "";
            }

            if (!id) throw new Error("Board was not created properly.");
            await advanceNSteps(id, steps);

        } catch (error) {
            console.log("Failed to advance N steps.", error);
        }
    }


    async function createBoard() {
        try {
            const response = await createNewBoard({ grid: grid });

            setBoardId(response);

            return response;
        } catch (err) {
            console.error("Failed to create board:", err);
        }
    }

    async function nextGeneration() {
        try {
            let id = boardId;
            if (!id) {
                id = await createBoard() ?? "";
            }

            if (!id) throw new Error("Board was not created properly.");

            const result = await getNextBoard(id);
            setGrid(result.grid);
            setGeneration(result.generation);
        } catch (err) {
            console.log("Error fetching next generation", err);
        }
    }

    function toggleSquare(r: number, c: number) {
        const newGrid = grid.map((row, i) =>
            row.map((cell, j) => (i === r && j === c ? (cell ? 0 : 1) : cell))
        );

        if (generation > 0) {
            setGeneration(0)
            setBoardId("");
        }

        setGrid(newGrid);
    }

    function clear() {
        setGrid(Array.from({ length: rows }, () => Array(cols).fill(0)));
        setGeneration(0);
        setBoardId("");
    }

    async function start() {
        let id = boardId;
        if (!id) {
            id = await createBoard() ?? "";
        }

        if (!id) throw new Error("Board was not created properly.");

        setIsRunning(true);

        startGame(id);
    }

    function stop() {
        setIsRunning(false);

        stopGame(boardId);
    }

    return {
        grid,
        toggleSquare,
        clear,
        isRunning,
        start,
        stop,
        generation,
        advanceSteps,
        nextGeneration,
    };
}
import type React from "react";
import { useGameOfLife } from "../hooks/useGameOfLife";
import { useState } from "react";

interface GridProps {
    rows?: number;
    columns?: number
};

const Grid: React.FC<GridProps> = ({ rows = 20, columns = 20 }) => {
    const {
        grid,
        toggleSquare,
        clear,
        isRunning,
        start,
        stop,
        generation,
        advanceGenerations } = useGameOfLife(rows, columns);

    const [advanceCount, setAdvanceCount] = useState<number | "">();

    return (
        <div className="flex flex-col items-center gap-3 mt-4">
            <div className="flex justify-between items-center w-full">
                <p className="text-gray-400 text-sm">Generation: {generation}</p>
            </div>

            <div
                className="grid border border-gray-700"
                style={{
                    gridTemplateColumns: `repeat(${columns}, 20px)`,
                }}
            >
                {grid.map((row, r) =>
                    row.map((val, c) => (
                        <div
                            key={`${r}-${c}`}
                            onClick={() => toggleSquare(r, c)}
                            className={`w-5 h-5 border border-gray-700 ${val ? "bg-green-400" : "bg-gray-800"
                                }`}
                        />
                    ))
                )}
            </div>

            <div className="flex justify-between w-full">
                <div className="flex items-start gap-2">
                    <button
                        onClick={() => {
                            advanceGenerations(advanceCount === "" ? 1 : Number(advanceCount));
                            setAdvanceCount("");
                        }}
                        disabled={isRunning || Number(advanceCount) < 0}
                        className={`px-4 py-1 rounded text-white transition ${isRunning
                            ? "bg-gray-500 cursor-not-allowed opacity-60"
                            : "bg-blue-500 hover:bg-blue-600"
                            }`}
                    >
                        Next
                    </button>

                    <input
                        type="number"
                        id="steps"
                        value={advanceCount}
                        onChange={(e) =>
                            setAdvanceCount(e.target.value === "" ? "" : Number(e.target.value))
                        }
                        placeholder="Steps"
                        className="w-20 px-1 py-1 text-black rounded border border-gray-500 text-center"
                        disabled={isRunning}
                    />
                </div>

                <div className="flex gap-2">
                    <button
                        className="px-4 py-1 rounded text-white bg-blue-600 hover:bg-blue-700 transition"
                        onClick={isRunning ? stop : start}
                    >
                        {isRunning ? "Stop" : "Start"}
                    </button>
                    <button
                        className={`px-4 py-1 rounded text-white ${isRunning
                            ? "bg-gray-500 cursor-not-allowed opacity-60"
                            : "bg-gray-400 hover:bg-gray-500"
                            }`}
                        onClick={clear}
                        disabled={isRunning}
                    >
                        Clear
                    </button>
                </div>
            </div>
        </div>

    );
}

export default Grid;
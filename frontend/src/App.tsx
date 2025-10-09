import Grid from './components/Grid'

function App() {
  return (
    <div className='flex flex-col items-center justify-center min-h-screen'>
      <h1 className='text-3xl font-bold mb-6'>Game of Life</h1>
      <Grid rows={20} columns={20} />
    </div>
  )
}

export default App
